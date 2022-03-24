using NotSoBoring.Domain.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotSoBoring.Domain.Utils
{
    public static class ReplyMarkupFactory
    {
        public static IReplyMarkup GetDefaultKeyboardReplyMarkup()
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { StringUtils.Strings.ConnectMeToAnAnnonymous },
                    //new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        public static IReplyMarkup GetInSessionKeyboardReplyMarkup()
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { StringUtils.Strings.CancelSession },
                })
            {
                ResizeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        public static IReplyMarkup GetWaitingForMatchKeyboardReplyMarkup()
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { StringUtils.Strings.CancelRequest },
                })
            {
                ResizeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        public static IReplyMarkup GetUserReplyMarkup(UserState userState)
        {
            return userState switch
            {
                UserState.InMenu => GetDefaultKeyboardReplyMarkup(),
                UserState.WaitingForMatch => GetWaitingForMatchKeyboardReplyMarkup(),
                UserState.InSession => GetInSessionKeyboardReplyMarkup(),
                _ => GetDefaultKeyboardReplyMarkup()
            };
        }
    }
}
