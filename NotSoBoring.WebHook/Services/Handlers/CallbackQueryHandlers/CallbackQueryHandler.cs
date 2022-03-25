using NotSoBoring.Domain.Utils;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;

        public CallbackQueryHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public Func<Task> HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            Func<Task> action = callbackQuery.Data switch
            {
                StringUtils.InlineKeyboard.EditProfile => async () => await EditProfile(callbackQuery),
                _ => () => Task.CompletedTask
            };

            return action;
        }

        private async Task EditProfile(CallbackQuery callbackQuery)
        {
            var replyMarkup = ReplyMarkupFactory.GetEditProfileInlineKeyboard();

            await _botClient.EditMessageReplyMarkupAsync(chatId: callbackQuery.From.Id,
                                                         messageId: callbackQuery.Message.MessageId,
                                                         replyMarkup: (InlineKeyboardMarkup) replyMarkup);
        }
    }
}