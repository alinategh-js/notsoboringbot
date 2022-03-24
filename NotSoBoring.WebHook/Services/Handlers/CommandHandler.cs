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

        public CommandHandler(MatchingEngine matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public async Task<Message> ConnectToAnonymous(ITelegramBotClient bot, Message message)
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

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId);
        }

        public async Task<Message> CancelRequest(ITelegramBotClient bot, Message message)
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

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId,
                                                      replyMarkup: replyMarkup);
        }

        public async Task<Message> CancelSession(ITelegramBotClient bot, Message message)
        {
            var userId = message.From.Id;
            string firstText = "شما در حال حاضر چت فعال ندارید.";
            string secondText = "";
            if(_matchingEngine.TryCancelSession(userId, out long secondUserId))
            {
                firstText = "چت با مخاطب توسط شما قطع شد.";
                secondText = "چت توسط مخاطب شما قطع شد.";

                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();

                await bot.SendTextMessageAsync(chatId: userId,
                                                      text: firstText,
                                                      replyMarkup: replyMarkup);

                return await bot.SendTextMessageAsync(chatId: secondUserId,
                                                      text: secondText,
                                                      replyMarkup: replyMarkup);
            }
            else
            {
                return await bot.SendTextMessageAsync(chatId: userId,
                                                      text: firstText);
            }
        }
    }
}
