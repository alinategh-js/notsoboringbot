using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.DTOs;
using NotSoBoring.Matchmaking;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking.Users;
using Microsoft.Extensions.Configuration;
using NotSoBoring.Domain.Extensions;
using System.ComponentModel.DataAnnotations;
using NotSoBoring.Domain.Enums;
using System;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers
{
    public class CommandHandler
    {
        private readonly MatchingEngine _matchingEngine;
        private readonly UserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;

        public CommandHandler(MatchingEngine matchingEngine, UserService userService, ITelegramBotClient botClient
            , IConfiguration configuration)
        {
            _matchingEngine = matchingEngine;
            _userService = userService;
            _botClient = botClient;
            _configuration = configuration;
        }

        public async Task ConnectToAnonymous(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return;

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

            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId);
        }

        public async Task CancelRequest(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return;

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

            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId,
                                                      replyMarkup: replyMarkup);
        }

        public async Task CancelSession(Message message)
        {
            var userId = message.From.Id;
            string firstText = "شما در حال حاضر چت فعال ندارید.";
            string secondText = "";
            if (_matchingEngine.TryCancelSession(userId, out long secondUserId))
            {
                firstText = "چت با مخاطب توسط شما قطع شد.";
                secondText = "چت توسط مخاطب شما قطع شد.";

                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: firstText,
                                                      replyMarkup: replyMarkup);

                await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: secondText,
                                                      replyMarkup: replyMarkup);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: firstText);
            }
        }

        public async Task ShowProfile(Message message, long? anotherUserId = null)
        {
            var userId = message.From.Id;
            var user = await _userService.GetUser(userId);
            if (user != null)
            {
                var replyMarkup = ReplyMarkupFactory.GetUserProfileInlineKeyboard();
                string nickname = user.Nickname ?? "❌";
                string age = user.Age?.ToString() ?? "❌";
                string gender = user.Gender != null ? user.Gender.GetAttribute<DisplayAttribute>()?.Name : "❌";
                string photo = user.Photo ?? _configuration["BotImages:DefaultProfileImage"];
                string caption = $"نام مستعار: {nickname}\n"
                                 + $"جنسیت: {gender}\n"
                                 + $"سن: {age}\n";

                await _botClient.SendPhotoAsync(chatId: userId,
                                                photo: photo,
                                                caption: caption,
                                                replyToMessageId: message.MessageId,
                                                replyMarkup: replyMarkup);
            }
        }

        public async Task EditNickname(Message message)
        {
            var userId = message.From.Id;
            var newNickname = message.Text;
            await _userService.EditUserNickname(userId, newNickname);
            _userService.ChangeUserState(userId, UserState.InMenu);

            string text = $"نام مستعار شما با موفقیت به \"{newNickname}\" تغییر یافت ✔️";
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text);
        }

        public async Task EditAge(Message message)
        {
            string text = "";
            if (!Int32.TryParse(message.Text.Trim(), out int newAge) || newAge > 99 || newAge < 1)
            {
                text = "لطفا عددی بین 1 تا 99 وارد کنید (کاراکتر های انگلیسی)";
                await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                                                  text: text);
                return;
            }

            var userId = message.From.Id;
            await _userService.EditAge(userId, newAge);
            _userService.ChangeUserState(userId, UserState.InMenu);

            text = $"سن شما با موفقیت به \"{newAge}\" تغییر یافت ✔️";
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text);
        }

        public async Task EditProfilePhoto(Message message)
        {
            if (message.Photo == null)
                return;

            var userId = message.From.Id;
            await _userService.EditPhoto(userId, message.Photo[0].FileId);
            _userService.ChangeUserState(userId, UserState.InMenu);

            string text = "عکس پروفایل شما با موفقیت تغییر یافت ✔️";
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text);
        }
    }
}