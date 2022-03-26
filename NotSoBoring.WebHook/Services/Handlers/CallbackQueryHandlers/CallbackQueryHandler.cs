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

namespace NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserService _userService;

        public CallbackQueryHandler(ITelegramBotClient botClient, UserService userService)
        {
            _botClient = botClient;
            _userService = userService;
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
                    StringUtils.InlineKeyboard.Male => async () => await ChangeGender(callbackQuery, GenderTypes.Male),
                    StringUtils.InlineKeyboard.Female => async () => await ChangeGender(callbackQuery, GenderTypes.Female),
                    _ => () => Task.CompletedTask
                };
            }
            else
            {
                action = async () => await CantEditProfile(callbackQuery);
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

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text);
        }

        private async Task EditAge(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingAge);
            string text = "لطفا سن خود را وارد کنید 👇";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text);
        }

        private async Task EditProfilePhoto(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingPhoto);
            string text = "لطفا عکس خود را آپلود کنید 👇";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text);
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
        }

        private async Task ChangeGender(CallbackQuery callbackQuery, GenderTypes gender)
        {
            var userId = callbackQuery.From.Id;
            _userService.ChangeUserState(userId, UserState.InMenu);
            await _userService.EditGender(userId, gender);

            var genderName = gender.GetAttribute<DisplayAttribute>().Name;
            string text = $"جنسیت شما با موفقیت به \"{genderName}\" تغییر یافت ✔️";
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
    }
}