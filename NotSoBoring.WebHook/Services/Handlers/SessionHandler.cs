using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers
{
    public class SessionHandler
    {
        private readonly MatchingEngine _matchingEngine;
        private readonly ITelegramBotClient _botClient;

        public SessionHandler(MatchingEngine matchingEngine, ITelegramBotClient botClient)
        {
            _matchingEngine = matchingEngine;
            _botClient = botClient;
        }

        public async Task<Message> SendSessionTextMessage(Message message)
        {
            var userId = message.From.Id;
            if(_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                return await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: message.Text,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }

            return await Task.FromResult(message);
        }

        public async Task<Message> SendSessionStickerMessage(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                return await _botClient.SendStickerAsync(chatId: secondUserId,
                                                      sticker: message.Sticker?.FileId,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }

            return await Task.FromResult(message);
        }

        public async Task<Message> SendSessionVoicMessage(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                return await _botClient.SendVoiceAsync(chatId: secondUserId,
                                                      voice: message.Voice?.FileId,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }

            return await Task.FromResult(message);
        }
    }
}
