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
using Telegram.Bot.Types.ReplyMarkups;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers
{
    public class CommandHandler
    {
        private readonly MatchingEngine _matchingEngine;
        private readonly UserService _userService;
        private readonly ContactService _contactService;
        private readonly DirectMessageService _directMessageService;
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;

        public CommandHandler(MatchingEngine matchingEngine, UserService userService, ITelegramBotClient botClient
            , IConfiguration configuration, ContactService contactService, DirectMessageService directMessageService)
        {
            _matchingEngine = matchingEngine;
            _userService = userService;
            _botClient = botClient;
            _configuration = configuration;
            _contactService = contactService;
            _directMessageService = directMessageService;

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
            if (_matchingEngine.IsUserInSession(userId, out var secondUserId))
            {
                var targetUser = await _userService.GetUser(secondUserId);
                await ShowProfile(message, targetUser.UniqueId);
            }
        }

        public async Task ShowProfile(Message message, string anotherUniqueId = null)
        {
            var userId = message.From.Id;
            var user = await _userService.GetUser(userId);
            bool selfProfile = false;
            ApplicationUser targetUser;
            if (anotherUniqueId == null)
            {
                targetUser = user;
                selfProfile = true;
            }
            else
            {
                targetUser = await _userService.GetUser(anotherUniqueId);
            }

            if (targetUser != null)
            {
                if(targetUser.Id == userId)
                    selfProfile = true;

                var isInContacts = false;
                if (!selfProfile)
                {
                    isInContacts = await _contactService.IsUserInContacts(userId, targetUser.Id);
                }
                var replyMarkup = ReplyMarkupFactory.GetUserProfileInlineKeyboard(selfProfile, isInContacts: isInContacts, userId: targetUser.Id);
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
                if (targetUser != user && targetUser.Latitude.HasValue && targetUser.Longitude.HasValue
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

        public async Task CancelOperation(Message message)
        {
            _userService.ChangeUserState(message.From.Id, UserState.InMenu);
            string text = "عملیات لغو شد. 👍";

            await _botClient.DeleteMessageAsync(chatId: message.From.Id, messageId: message.MessageId);

            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task CompleteAddToContactsRequest(Message message)
        {
            var userId = message.From.Id;

            var contactName = message.Text.Trim();

            if (_contactService.GetContactIdFromRequests(userId, out var contactId))
            {
                var text = "";
                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
                _userService.ChangeUserState(message.From.Id, UserState.InMenu);
                if(await _contactService.AddUserToContacts(userId, contactId, contactName))
                    text = "کاربر موردنظر با موفقیت به لیست مخاطبین شما اضافه شد. 👍";
                else
                    text = "عملیات موردنظر با خطا مواجه شد.";

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                    text: text,
                                                    replyMarkup: replyMarkup);
            }
        }

        public async Task CompleteSendDirectMessageRequest(Message message)
        {
            var userId = message.From.Id;

            var messageText = message.Text.Trim();

            if (_directMessageService.GetTargetUserId(userId, out var targetUserId))
            {
                var text = "پیام دایرکت با موفقیت ارسال شد.";
                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
                _userService.ChangeUserState(message.From.Id, UserState.InMenu);
                var uniqueId = (await _userService.GetUser(userId)).UniqueId;

                await _botClient.SendTextMessageAsync(chatId: targetUserId.Value,
                                                      text: StringUtils.DirectMessage.DirectMessageText(messageText, uniqueId));

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                    text: text,
                                                    replyMarkup: replyMarkup);
            }
        }
    }
}