using Microsoft.Extensions.Logging;
using NotSoBoring.Matchmaking.Users;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies;
using NotSoBoring.Domain.Enums;
using NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace NotSoBoring.WebHook.Services
{
    public class HandleUpdateService
    {
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        private readonly BotConfiguration _botConfig;

        public HandleUpdateService(ITelegramBotClient botClient,
            ILogger<HandleUpdateService> logger,
            UserService userService, IServiceProvider serviceProvider,
            CallbackQueryHandler callbackQueryHandler, IConfiguration configuration)
        {
            _botClient = botClient;
            _logger = logger;
            _userService = userService;
            _serviceProvider = serviceProvider;
            _callbackQueryHandler = callbackQueryHandler;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
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

            (bool result, var userState) = await CheckUser(message.From.Id);
            if (!result) return;

            Func<Task> action = MessageHandlerFactory.GetMessageHandler(message.Type).HandleMessage(message, userState, _serviceProvider);

            await action.Invoke();
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            if (callbackQuery.From == null)
                return;

            (bool result, var userState) = await CheckUser(callbackQuery.From.Id);
            if (!result) return;

            Func<Task> action = _callbackQueryHandler.HandleCallbackQuery(callbackQuery, userState);

            await action.Invoke();
        }

        private async Task<(bool, UserState)> CheckUser(long userId)
        {
            // check if user is a member of our channel
            string shouldJoinChannel = "برای استفاده از ربات باید عضو کانال ما باشید. \n\n"
                                     + $"آیدی چنل : 👈 {_botConfig.TelegramChannel}";

            try
            {
                var chatMember = await _botClient.GetChatMemberAsync(_botConfig.TelegramChannel, userId);
                if (chatMember == null || chatMember.Status == ChatMemberStatus.Left 
                    || chatMember.Status == ChatMemberStatus.Kicked || chatMember.Status == ChatMemberStatus.Restricted)
                {
                    await _botClient.SendTextMessageAsync(chatId: userId,
                                                          text: shouldJoinChannel);

                    return (false, UserState.InMenu);
                }
            }
            catch
            {
                await _botClient.SendTextMessageAsync(chatId: userId,
                                                          text: shouldJoinChannel);

                return (false, UserState.InMenu);
            }

            // check if user exists
            var user = await _userService.GetUser(userId);
            if (user == null)
            {
                // register user
                await _userService.AddUser(userId);
            }

            // update user recent activity
            _userService.UpdateUserRecentActivity(userId);

            var userState = _userService.GetUserState(userId);
            return (true, userState);
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
            Log.Information("HandleUpdateService:HandleErrorAsync : {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
