using Microsoft.Extensions.Logging;
using NotSoBoring.Matchmaking;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.WebHook.Services.Handlers;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies;
using NotSoBoring.Domain.Enums;
using NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers;

namespace NotSoBoring.WebHook.Services
{
    public class HandleUpdateService
    {
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly UserService _userService;
        private readonly MatchingEngine _matchingEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly CallbackQueryHandler _callbackQueryHandler;

        public HandleUpdateService(ILogger<HandleUpdateService> logger,
            UserService userService, MatchingEngine matchingEngine, IServiceProvider serviceProvider,
            CallbackQueryHandler callbackQueryHandler)
        {
            _logger = logger;
            _userService = userService;
            _matchingEngine = matchingEngine;
            _serviceProvider = serviceProvider;
            _callbackQueryHandler = callbackQueryHandler;
        }

        public async Task EchoAsync(Update update)
        {
            Func<Task> handler = update.Type switch
            {
                UpdateType.Message => async () => await BotOnMessageReceived(update.Message!),
                UpdateType.CallbackQuery => async () => await BotOnCallbackQueryReceived(update.CallbackQuery),
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

            var userState = await CheckUser(message.From.Id);

            Func<Task> action = MessageHandlerFactory.GetMessageHandler(message.Type).HandleMessage(message, userState, _serviceProvider);

            await action.Invoke();
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            if (callbackQuery.From == null)
                return;

            var userState = await CheckUser(callbackQuery.From.Id);

            Func<Task> action = _callbackQueryHandler.HandleCallbackQuery(callbackQuery);

            await action.Invoke();
        }

        private async Task<UserState> CheckUser(long userId)
        {
            // check if user exists
            var user = await _userService.GetUser(userId);
            if (user == null)
            {
                // register user
                await _userService.AddUser(userId);
            }

            var userState = _matchingEngine.GetUserState(userId);
            return userState;
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
