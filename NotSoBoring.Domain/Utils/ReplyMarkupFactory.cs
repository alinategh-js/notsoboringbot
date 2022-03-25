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
                    new KeyboardButton[] { StringUtils.Keyboard.ConnectMeToAnAnonymous },
                    new KeyboardButton[] { StringUtils.Keyboard.Profile },
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
                    new KeyboardButton[] { StringUtils.Keyboard.CancelSession },
                })
            {
                ResizeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        //public static IReplyMarkup GetWaitingForMatchKeyboardReplyMarkup()
        //{
        //    ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
        //        new[]
        //        {
        //            new KeyboardButton[] { StringUtils.Strings.CancelRequest },
        //        })
        //    {
        //        ResizeKeyboard = true
        //    };

        //    return replyKeyboardMarkup;
        //}

        public static IReplyMarkup GetUserReplyMarkup(UserState userState)
        {
            return userState switch
            {
                UserState.InMenu => GetDefaultKeyboardReplyMarkup(),
                UserState.WaitingForMatch => GetDefaultKeyboardReplyMarkup(),
                UserState.InSession => GetInSessionKeyboardReplyMarkup(),
                _ => GetDefaultKeyboardReplyMarkup()
            };
        }

        public static IReplyMarkup GetUserProfileInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                    new InlineKeyboardButton[] { StringUtils.InlineKeyboard.EditProfile }
                });

            return inlineKeyboardMarkup;
        }

        public static IReplyMarkup GetEditProfileInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.EditGender,
                            StringUtils.InlineKeyboard.EditNickname
                        },
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.EditProfilePhoto,
                            StringUtils.InlineKeyboard.EditAge
                        },
                });

            return inlineKeyboardMarkup;
        }
    }
}
