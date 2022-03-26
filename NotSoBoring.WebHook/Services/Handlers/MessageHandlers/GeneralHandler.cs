using NotSoBoring.Domain.Utils;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers
{
    public class GeneralHandler
    {
        private readonly ITelegramBotClient _botClient;

        public GeneralHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task<Message> Usage(Message message)
        {
            const string usage = "نگرفتم چی گفتی 🤔\n\n" +
                                 "از منوی پایین میتونی انتخاب کنی تا کمکت کنم 👇";

            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: usage,
                                                  replyToMessageId: message.MessageId,
                                                  replyMarkup: replyMarkup);
        }

        public async Task<Message> InvalidInput(Message message)
        {
            const string text = StringUtils.Errors.InvalidInput;

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: text,
                                                  replyToMessageId: message.MessageId);
        }
    }
}
