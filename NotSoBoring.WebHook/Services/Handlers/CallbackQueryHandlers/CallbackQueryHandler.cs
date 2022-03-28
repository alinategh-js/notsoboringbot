using NotSoBoring.Core.Enums;
using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking.Users;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using NotSoBoring.Domain.Extensions;
using System.ComponentModel.DataAnnotations;
using NotSoBoring.Matchmaking;
using NotSoBoring.Domain.DTOs;

namespace NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserService _userService;
        private readonly MatchingEngine _matchingEngine;

        public CallbackQueryHandler(ITelegramBotClient botClient, UserService userService, MatchingEngine matchingEngine)
        {
            _botClient = botClient;
            _userService = userService;
            _matchingEngine = matchingEngine;
        }

        public Func<Task> HandleCallbackQuery(CallbackQuery callbackQuery, UserState userState)
        {
            Func<Task> action;
            if (userState != UserState.InSession)
            {
                action = callbackQuery.Data switch
                {
                    StringUtils.InlineKeyboard.EditProfile => async () => await EditProfile(callbackQuery),
                    StringUtils.InlineKeyboard.EditNickname => async () => await EditNickname(callbackQuery),
                    StringUtils.InlineKeyboard.EditAge => async () => await EditAge(callbackQuery),
                    StringUtils.InlineKeyboard.EditProfilePhoto => async () => await EditProfilePhoto(callbackQuery),
                    StringUtils.InlineKeyboard.EditGender => async () => await EditGender(callbackQuery),
                    StringUtils.InlineKeyboard.EditLocation => async () => await EditLocation(callbackQuery),
                    StringUtils.InlineKeyboard.Male => async () => await ChangeGender(callbackQuery, GenderTypes.Male),
                    StringUtils.InlineKeyboard.Female => async () => await ChangeGender(callbackQuery, GenderTypes.Female),
                    StringUtils.InlineKeyboard.DontCareGender => async () => await ConnectToAnonymous(callbackQuery),
                    StringUtils.InlineKeyboard.OnlyMales => async () => await ConnectToAnonymous(callbackQuery, GenderTypes.Male),
                    StringUtils.InlineKeyboard.OnlyFemales => async () => await ConnectToAnonymous(callbackQuery, GenderTypes.Female),
                    _ => () => Task.CompletedTask
                };
            }
            else // in session
            {
                action = callbackQuery.Data switch
                {
                    StringUtils.InlineKeyboard.EndChat => async () => await EndSession(callbackQuery),
                    StringUtils.InlineKeyboard.ContinueChat => async () => await ContinueSession(callbackQuery),
                    StringUtils.InlineKeyboard.EditProfile or
                    StringUtils.InlineKeyboard.EditNickname or
                    StringUtils.InlineKeyboard.EditAge or
                    StringUtils.InlineKeyboard.EditGender or
                    StringUtils.InlineKeyboard.EditProfilePhoto => async () => await CantEditProfile(callbackQuery),
                    _ => () => Task.CompletedTask
                };
            }

            return action;
        }

        private async Task EditProfile(CallbackQuery callbackQuery)
        {
            var replyMarkup = ReplyMarkupFactory.GetEditProfileInlineKeyboard();

            await _botClient.EditMessageReplyMarkupAsync(chatId: callbackQuery.From.Id,
                                                         messageId: callbackQuery.Message.MessageId,
                                                         replyMarkup: (InlineKeyboardMarkup) replyMarkup);
        }

        private async Task EditNickname(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingNickname);
            string text = "لطفا نام مستعار خود را وارد کنید 👇";

            var replyMarkup = ReplyMarkupFactory.GetEditProfileKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditAge(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingAge);
            string text = "لطفا سن خود را وارد کنید 👇";

            var replyMarkup = ReplyMarkupFactory.GetEditProfileKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditProfilePhoto(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingPhoto);
            string text = "لطفا عکس خود را آپلود کنید 👇";

            var replyMarkup = ReplyMarkupFactory.GetEditProfileKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditGender(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingGender);
            string text = "لطفا جنسیت خود را انتخاب کنید 👇";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);

            var replyMarkup = ReplyMarkupFactory.GetEditGenderInlineKeyboard();
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);

            var replyMarkupKeyboard = ReplyMarkupFactory.GetEditProfileKeyboard();
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: "",
                                                  replyMarkup: replyMarkupKeyboard);
        }

        private async Task EditLocation(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingLocation);
            string text = "لطفا لوکیشن خود را بفرستید 👇";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);

            var replyMarkup = ReplyMarkupFactory.GetEditLocationKeyboard();
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task ChangeGender(CallbackQuery callbackQuery, GenderTypes gender)
        {
            var userId = callbackQuery.From.Id;
            _userService.ChangeUserState(userId, UserState.InMenu);
            await _userService.EditGender(userId, gender);

            var genderName = gender.GetAttribute<DisplayAttribute>().Name;
            string text = $"جنسیت شما با موفقیت به \"{genderName}\" تغییر یافت ✔️";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);

            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text);
        }

        private async Task CantEditProfile(CallbackQuery callbackQuery)
        {
            string text = StringUtils.Errors.CantEditProfileInSession;
            await _botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                                      text: text,
                                                      showAlert: true);
        }

        private async Task EndSession(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;
            if (_matchingEngine.TryCancelSession(userId, out long secondUserId))
            {
                string firstText = "چت با مخاطب توسط شما قطع شد.";
                string secondText = "چت توسط مخاطب شما قطع شد.";

                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();

                await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id,
                                                messageId: callbackQuery.Message.MessageId);

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: firstText,
                                                      replyMarkup: replyMarkup);

                await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: secondText,
                                                      replyMarkup: replyMarkup);
            }
        }

        private async Task ContinueSession(CallbackQuery callbackQuery)
        {
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id,
                                                messageId: callbackQuery.Message.MessageId);
        }

        private async Task ConnectToAnonymous(CallbackQuery callbackQuery, GenderTypes? genderPreferrence = null)
        {
            var userId = callbackQuery.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return;

            string text = "";
            if (_matchingEngine.TryAddRequest(new MatchRequest { UserId = userId, PreferredGender = genderPreferrence }))
            {
                text = "منتظر باش تا به یکی وصلت کنم 🕐 ";
            }
            else
            {
                text = "شما کمی پیش درخواست دادید، لطفا کمی صبر کنید تا به یک نفر متصل شوید.\n\n" +
                    "در غیر اینصورت میتوانید درخواست خود را با /cancel لغو کنید.";
            }
            await _botClient.DeleteMessageAsync(chatId: userId, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text);
        }
    }
}