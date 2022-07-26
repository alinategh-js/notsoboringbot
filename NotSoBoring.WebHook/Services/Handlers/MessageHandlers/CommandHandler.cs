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

            string text = "Who would you like to connect to?";
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
                text = "Your request was cancelled.";
            }
            else
            {
                text = "You don't have any active requests.";
            }

            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: text,
                                                      replyToMessageId: message.MessageId,
                                                      replyMarkup: replyMarkup);
        }

        public async Task CancelSession(Message message)
        {
            var userId = message.From.Id;
            string text = "You don't have an active chat at the moment.";
            if (_matchingEngine.IsUserInSession(userId))
            {
                text = "Are you sure you want to end the chat؟ ❔";

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
                string distanceString = "Unknown";
                if (targetUser != user && targetUser.Latitude.HasValue && targetUser.Longitude.HasValue
                    && user.Latitude.HasValue && user.Longitude.HasValue)
                {
                    distance = Math.Round(LocationUtils.CalculateDistance(targetUser.Latitude.Value, targetUser.Longitude.Value, user.Latitude.Value, user.Longitude.Value));
                    distanceString = LocationUtils.DistanceToString(distance);
                }
                bool showDistance = user != targetUser;
                string distanceFinalString = showDistance ? $"Distance: {distanceString}\n\n" : string.Empty;
                string caption = $"Nickname: {nickname}\n"
                                 + $"Gender: {gender}\n"
                                 + $"Age: {age}\n\n"
                                 + $"Status: {onlineStatus}\n\n"
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

            string text = $"Your nickname was changed to \"{newNickname}\" ✔️";
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
                text = "Please enter a number between 1 and 99";
                await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                                                  text: text);
                return;
            }

            var userId = message.From.Id;
            await _userService.EditAge(userId, newAge);
            _userService.ChangeUserState(userId, UserState.InMenu);

            text = $"Your age was changed to \"{newAge}\" ✔️";
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

            string text = "Your profile photo was successfully changed ✔️";
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

            string text = "Your location was successfully changed ✔️";
            var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        public async Task CancelOperation(Message message)
        {
            _userService.ChangeUserState(message.From.Id, UserState.InMenu);
            string text = "Operation is cancelled 👍";

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
                    text = "User was successfully added to your contacts 👍";
                else
                    text = "Something went wrong.";

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
                var text = "Direct message was successfully sent to the user.";
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