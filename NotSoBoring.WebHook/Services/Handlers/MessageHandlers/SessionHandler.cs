using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers
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

        public async Task SendTextMessage(Message message)
        {
            var userId = message.From.Id;
            if(_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: message.Text,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }
        }

        public async Task SendStickerMessage(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                await _botClient.SendStickerAsync(chatId: secondUserId,
                                                      sticker: message.Sticker?.FileId,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }
        }

        public async Task SendVoiceMessage(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                await _botClient.SendVoiceAsync(chatId: secondUserId,
                                                      voice: message.Voice?.FileId,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }
        }

        public async Task SendPhotoMessage(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                await _botClient.SendPhotoAsync(chatId: secondUserId,
                                                      photo: message.Photo[0]?.FileId,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }
        }
    }
}
