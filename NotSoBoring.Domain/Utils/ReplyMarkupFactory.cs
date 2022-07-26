using NotSoBoring.Domain.Enums;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotSoBoring.Domain.Utils
{
    public static class ReplyMarkupFactory
    {
        public static IReplyMarkup GetDefaultKeyboard()
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

        public static IReplyMarkup GetInSessionKeyboard()
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { StringUtils.Keyboard.SeeContactProfile },
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
                UserState.InMenu => GetDefaultKeyboard(),
                UserState.WaitingForMatch => GetDefaultKeyboard(),
                UserState.InSession => GetInSessionKeyboard(),
                _ => GetDefaultKeyboard()
            };
        }

        public static IReplyMarkup GetUserProfileInlineKeyboard(bool selfProfile, bool isInContacts = false, long? userId = null)
        {
            InlineKeyboardMarkup inlineKeyboardMarkup;
            if (selfProfile)
            {
                inlineKeyboardMarkup = new InlineKeyboardMarkup(
                     new[]
                     {
                        new InlineKeyboardButton[] { StringUtils.InlineKeyboard.MyContacts },
                        new InlineKeyboardButton[] { StringUtils.InlineKeyboard.EditProfile }
                     });
            }
            else
            {
                inlineKeyboardMarkup = new InlineKeyboardMarkup(
                     new[]
                     {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(StringUtils.InlineKeyboard.SendDirectMessage, StringUtils.InlineKeyboardCallbackData.SendDirectMessage(userId.Value)),

                            !isInContacts ?
                                InlineKeyboardButton.WithCallbackData(StringUtils.InlineKeyboard.AddToContacts, StringUtils.InlineKeyboardCallbackData.AddToContacts(userId.Value))
                                : InlineKeyboardButton.WithCallbackData(StringUtils.InlineKeyboard.RemoveFromContacts, StringUtils.InlineKeyboardCallbackData.RemoveFromContacts(userId.Value))
                        }
                     });
            }

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
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.EditLocation,
                        },
                });

            return inlineKeyboardMarkup;
        }

        public static IReplyMarkup GetEditGenderInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.Male,
                            StringUtils.InlineKeyboard.Female
                        }
                });

            return inlineKeyboardMarkup;
        }

        public static IReplyMarkup GetInOperationKeyboard()
        {
            ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { StringUtils.Keyboard.CancelOperation }
                });

            return keyboardMarkup;
        }

        public static IReplyMarkup GetEditLocationKeyboard()
        {
            ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[] { KeyboardButton.WithRequestLocation(StringUtils.Keyboard.SendMyLocation) },
                    new KeyboardButton[] { StringUtils.Keyboard.CancelOperation }
                });

            return keyboardMarkup;
        }

        public static IReplyMarkup GetEndSessionInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.ContinueChat,
                            StringUtils.InlineKeyboard.EndChat
                        }
                });

            return inlineKeyboardMarkup;
        }

        public static IReplyMarkup GetChooseChatPreferrenceInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.DontCareGender
                        },
                        new InlineKeyboardButton[]
                        {
                            StringUtils.InlineKeyboard.OnlyMales,
                            StringUtils.InlineKeyboard.OnlyFemales,
                        }
                });

            return inlineKeyboardMarkup;
        }

        public static IReplyMarkup GetContactsListInlineKeyboard(int? nextPageNumber, bool hasPreviousPage = false)
        {
            var buttons = new List<InlineKeyboardButton>();
            if (nextPageNumber.HasValue)
                buttons.Add(InlineKeyboardButton.WithCallbackData(StringUtils.InlineKeyboard.NextPage, StringUtils.InlineKeyboardCallbackData.ContactsListNextPage(nextPageNumber.Value)));

            if (hasPreviousPage)
                buttons.Add(InlineKeyboardButton.WithCallbackData(StringUtils.InlineKeyboard.PreviousPage, StringUtils.InlineKeyboardCallbackData.ContactsListNextPage(nextPageNumber.Value - 2)));

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(
                new[]
                {
                    buttons.ToArray()
                });

            return inlineKeyboardMarkup;
        }
    }
}
