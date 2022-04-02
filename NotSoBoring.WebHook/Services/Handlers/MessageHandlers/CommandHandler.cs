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
using NotSoBoring.Core.Models;

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

            string text = "دوست داری به چه کسی وصلت کنم؟";
            var replyMarkup = ReplyMarkupFactory.GetChooseChatPreferrenceInlineKeyboard();

            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup,
                                                  replyToMessageId: message.MessageId);
        }

        public async Task CancelRequest(Message message)
        {
            var userId = message.From.Id;
            if (_matchingEngine.IsUserInSession(userId))
                return;

            string text = "";
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
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
            string text = "شما در حال حاضر چت فعال ندارید.";
            if (_matchingEngine.IsUserInSession(userId))
            {
                text = "مطمئنی که میخوای چت رو تموم کنی؟ ❔";

                var replyMarkup = ReplyMarkupFactory.GetEndSessionInlineKeyboard();

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text,
                                                      replyMarkup: replyMarkup,
                                                      replyToMessageId: message.MessageId);
            }
            else
            {
                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text,
                                                      replyMarkup: replyMarkup);
            }
        }

        public async Task ShowContactProfile(Message message)
        {
            var userId = message.From.Id;
            if(_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var targetUser = await _userService.GetUser(secondUserId);
                await ShowProfile(message, targetUser.UniqueId);
            }
        }

        public async Task ShowProfile(Message message, string anotherUniqueId = null)
        {
            var userId = message.From.Id;
            var user = await _userService.GetUser(userId);
            ApplicationUser targetUser;
            if (anotherUniqueId == null)
                targetUser = user;
            else
            {
                targetUser = await _userService.GetUser(anotherUniqueId);
            }

            if (targetUser != null)
            {
                var replyMarkup = ReplyMarkupFactory.GetUserProfileInlineKeyboard(anotherUniqueId == user.UniqueId ? null : anotherUniqueId);
                string nickname = targetUser.Nickname ?? "❌";
                string age = targetUser.Age?.ToString() ?? "❌";
                string gender = targetUser.Gender != null ? targetUser.Gender.GetAttribute<DisplayAttribute>()?.Name : "❌";
                string photo = targetUser.Photo ?? _configuration["BotImages:DefaultProfileImage"];
                string uniqueId = targetUser.UniqueId;

                var lastActivity = _userService.GetUserRecentActivity(targetUser.Id);
                var isInSession = _matchingEngine.IsUserInSession(targetUser.Id);
                string onlineStatus = StringUtils.GetUserOnlineStatus(lastActivity, isInSession);

                double distance = 0;
                string distanceString = "نامشخص";
                if(targetUser != user && targetUser.Latitude.HasValue && targetUser.Longitude.HasValue
                    && user.Latitude.HasValue && user.Longitude.HasValue)
                {
                    distance = Math.Round(LocationUtils.CalculateDistance(targetUser.Latitude.Value, targetUser.Longitude.Value, user.Latitude.Value, user.Longitude.Value));
                    distanceString = LocationUtils.DistanceToString(distance);
                }
                bool showDistance = user != targetUser;
                string distanceFinalString = showDistance ? $"فاصله: {distanceString}\n\n" : string.Empty;
                string caption = $"نام مستعار: {nickname}\n"
                                 + $"جنسیت: {gender}\n"
                                 + $"سن: {age}\n\n"
                                 + $"وضعیت: {onlineStatus}\n\n"
                                 + distanceFinalString
                                 + $"🆔: /user_{uniqueId}";

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
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task EditAge(Message message)
        {
            string text = "";
            if (!int.TryParse(message.Text.Trim(), out int newAge) || newAge > 99 || newAge < 1)
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
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task EditProfilePhoto(Message message)
        {
            if (message.Photo == null)
                return;

            var userId = message.From.Id;
            await _userService.EditPhoto(userId, message.Photo[0].FileId);
            _userService.ChangeUserState(userId, UserState.InMenu);

            string text = "عکس پروفایل شما با موفقیت تغییر یافت ✔️";
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task EditLocation(Message message)
        {
            if (message.Location == null)
                return;

            var userId = message.From.Id;
            await _userService.EditLocation(userId, message.Location.Latitude, message.Location.Longitude);
            _userService.ChangeUserState(userId, UserState.InMenu);

            string text = "موقعیت مکانی شما با موفقیت تغییر یافت ✔️";
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task CancelEditProfile(Message message)
        {
            _userService.ChangeUserState(message.From.Id, UserState.InMenu);
            string text = "عملیات ویرایش پروفایل لغو شد. 👍";

            await _botClient.DeleteMessageAsync(chatId: message.From.Id, messageId: message.MessageId);

            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }
    }
}