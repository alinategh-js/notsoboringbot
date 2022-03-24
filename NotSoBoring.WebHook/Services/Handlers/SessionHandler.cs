using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers
{
    public class SessionHandler
    {
        private readonly MatchingEngine _matchingEngine;

        public SessionHandler(MatchingEngine matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public async Task<Message> SendSessionTextMessage(ITelegramBotClient bot, Message message)
        {
            var userId = message.From.Id;
            if(_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var replyMarkup = ReplyMarkupFactory.GetUserReplyMarkup(UserState.InSession);

                return await bot.SendTextMessageAsync(chatId: secondUserId,
                                                      text: message.Text,
                                                      replyToMessageId: message.ReplyToMessage?.MessageId,
                                                      replyMarkup: replyMarkup);
            }

            return await Task.FromResult(message);
        }
    }
}
