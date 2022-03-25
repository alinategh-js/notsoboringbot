using Microsoft.Extensions.Logging;
using NotSoBoring.Matchmaking;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.WebHook.Services.Handlers;
using NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NotSoBoring.WebHook.Services
{
    public class HandleUpdateService
    {
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly UserService _userService;
        private readonly MatchingEngine _matchingEngine;
        private readonly IServiceProvider _serviceProvider;

        public HandleUpdateService(ILogger<HandleUpdateService> logger,
            UserService userService, MatchingEngine matchingEngine, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _userService = userService;
            _matchingEngine = matchingEngine;
            _serviceProvider = serviceProvider;
        }

        public async Task EchoAsync(Update update)
        {
            Func<Task> handler = update.Type switch
            {
                UpdateType.Message => async () => await BotOnMessageReceived(update.Message!),
                _ => async () => await UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler.Invoke();
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message.From == null)
                return;

            #region Check if user exists
            var user = await _userService.GetUser(message.From.Id);
            if (user == null)
            {
                // register user
                await _userService.AddUser(message.From.Id);
            }
            #endregion

            #region Get User's state
            var userState = _matchingEngine.GetUserState(message.From.Id);
            #endregion

            #region Determine action from user's state
            Func<Task<Message>> action = MessageHandlerFactory.GetMessageHandler(message.Type).HandleMessage(message, userState, _serviceProvider);
            #endregion

            Message sentMessage = await action.Invoke();
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);

        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {updateType}", update.Type);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
