using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.DTOs;
using NotSoBoring.Matchmaking;
using NotSoBoring.Domain.Utils;

namespace NotSoBoring.WebHook.Services.Handlers
{
    public class CommandHandler
    {
        private readonly MatchingEngine _matchingEngine;
        private readonly ITelegramBotClient _botClient;

        public CommandHandler(MatchingEngine matchingEngine, ITelegramBotClient botClient)
        {
            _matchingEngine = matchingEngine;
            _botClient = botClient;
        }

        public async Task<Message> ConnectToAnonymous(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return await Task.FromResult(message);

            string text = "";
            if (_matchingEngine.TryAddRequest(new MatchRequest { UserId = userId }))
            {
                text = "منتظر باش تا به یکی وصلت کنم 🕐 ";
            }
            else
            {
                text = "شما کمی پیش درخواست دادید، لطفا کمی صبر کنید تا به یک نفر متصل شوید.\n\n" + 
                    "در غیر اینصورت میتوانید درخواست خود را با /cancel لغو کنید.";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId);
        }

        public async Task<Message> CancelRequest(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return await Task.FromResult(message);

            string text = "";
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();
            if (_matchingEngine.TryCancelRequest(userId))
            {
                text = "درخواستی که داده بودی لغو شد.";
            }
            else
            {
                text = "شما درخواست فعالی ندارید.";
            }

            return await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId,
                                                      replyMarkup: replyMarkup);
        }

        public async Task<Message> CancelSession(Message message)
        {
            var userId = message.From.Id;
            string firstText = "شما در حال حاضر چت فعال ندارید.";
            string secondText = "";
            if(_matchingEngine.TryCancelSession(userId, out long secondUserId))
            {
                firstText = "چت با مخاطب توسط شما قطع شد.";
                secondText = "چت توسط مخاطب شما قطع شد.";

                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: firstText,
                                                      replyMarkup: replyMarkup);

                return await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: secondText,
                                                      replyMarkup: replyMarkup);
            }
            else
            {
                return await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: firstText);
            }
        }
    }
}
