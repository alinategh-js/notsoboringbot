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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using System.Linq;
using System.Text;

namespace NotSoBoring.WebHook.Services.Handlers.CallbackQueryHandlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UserService _userService;
        private readonly MatchingEngine _matchingEngine;
        private readonly ContactService _contactService;
        private readonly DirectMessageService _directMessageService;

        public CallbackQueryHandler(ITelegramBotClient botClient, UserService userService,
            MatchingEngine matchingEngine, ContactService contactService,
            DirectMessageService directMessageService)
        {
            _botClient = botClient;
            _userService = userService;
            _matchingEngine = matchingEngine;
            _contactService = contactService;
            _directMessageService = directMessageService;
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
                    StringUtils.InlineKeyboard.MyContacts => async () => await GetMyContacts(callbackQuery),
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.SendDirectMessagePrefix) => async () => await SendDirectMessage(callbackQuery),
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.ContactsListNextPagePrefix) => async () => await GetMyContacts(callbackQuery),
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.AddToContactsPrefix) => async () => await AddToContacts(callbackQuery),
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.RemoveFromContactsPrefix) => async () => await RemoveFromContacts(callbackQuery),
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
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.AddToContactsPrefix) => async () => await CantAddToContacts(callbackQuery),
                    string s when s.StartsWith(StringUtils.InlineKeyboardCallbackData.RemoveFromContactsPrefix) => async () => await RemoveFromContacts(callbackQuery),
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
                                                         replyMarkup: (InlineKeyboardMarkup)replyMarkup);
        }

        private async Task EditNickname(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingNickname);
            string text = "Please enter your Nickname 👇";

            var replyMarkup = ReplyMarkupFactory.GetInOperationKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditAge(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingAge);
            string text = "Please enter your Age 👇";

            var replyMarkup = ReplyMarkupFactory.GetInOperationKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditProfilePhoto(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingPhoto);
            string text = "Please upload a profile picture 👇";

            var replyMarkup = ReplyMarkupFactory.GetInOperationKeyboard();
            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task EditGender(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingGender);
            string text = "Please choose your Gender 👇";

            await _botClient.DeleteMessageAsync(chatId: callbackQuery.From.Id, messageId: callbackQuery.Message.MessageId);

            var replyMarkup = ReplyMarkupFactory.GetEditGenderInlineKeyboard();
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: text,
                                                  replyMarkup: replyMarkup);

            var replyMarkupKeyboard = ReplyMarkupFactory.GetInOperationKeyboard();
            await _botClient.SendTextMessageAsync(chatId: callbackQuery.From.Id,
                                                  text: "",
                                                  replyMarkup: replyMarkupKeyboard);
        }

        private async Task EditLocation(CallbackQuery callbackQuery)
        {
            _userService.ChangeUserState(callbackQuery.From.Id, UserState.EditingLocation);
            string text = "Please send your Location 👇";

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
            string text = $"Your gender was changed to \"{genderName}\" ✔️";

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

        private async Task CantAddToContacts(CallbackQuery callbackQuery)
        {
            string text = StringUtils.Errors.CantAddContactsInSession;
            await _botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                                      text: text,
                                                      showAlert: true);
        }

        private async Task EndSession(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;
            if (_matchingEngine.TryCancelSession(userId, out long secondUserId))
            {
                string firstText = "Chat was ended by you.";
                string secondText = "Chat was ended by them.";

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

            (bool result, string text) = await _matchingEngine.TryAddRequest(new MatchRequest { UserId = userId, PreferredGender = genderPreferrence });

            await _botClient.DeleteMessageAsync(chatId: userId, messageId: callbackQuery.Message.MessageId);
            await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text);
        }

        private async Task AddToContacts(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;

            if (long.TryParse(StringUtils.InlineKeyboardCallbackData.GetAddToContactsContactId(callbackQuery.Data), out long contactId))
            {
                _userService.ChangeUserState(userId, UserState.AddingToContacts);
                _contactService.AddUserToContactsRequest(userId, contactId);

                var replyMarkup = ReplyMarkupFactory.GetInOperationKeyboard();

                var text = "Please choose a nickname for your contact to save 👇";

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text,
                                                      replyMarkup: replyMarkup);
            }
        }

        private async Task RemoveFromContacts(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;

            if (long.TryParse(StringUtils.InlineKeyboardCallbackData.GetRemoveFromContactsContactId(callbackQuery.Data), out long contactId))
            {
                bool result = await _contactService.RemoveUserFromContacts(userId, contactId);
                if (result)
                {
                    var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboard();

                    var text = "User was removed from your contacts 👍";

                    await _botClient.DeleteMessageAsync(chatId: userId, messageId: callbackQuery.Message.MessageId);
                    await _botClient.SendTextMessageAsync(chatId: userId,
                                                          text: text,
                                                          replyMarkup: replyMarkup);
                }
            }
        }

        private async Task GetMyContacts(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;
            int pageSize = 10;

            if (!int.TryParse(StringUtils.InlineKeyboardCallbackData.GetContactsListNextPageNumber(callbackQuery.Data), out int pageNumber))
                pageNumber = 1;

            var contacts = await _contactService.GetUserContacts(userId);
            contacts = contacts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            if (contacts.Count == 0)
            {
                string error = StringUtils.Errors.NoMoreResultToShow;
                await _botClient.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                                          text: error,
                                                          showAlert: true);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Your contacts:");
            sb.AppendLine();

            foreach (var contact in contacts)
            {
                sb.AppendLine("----------------------");
                var contactInfo = $"{contact.ContactUser.Nickname} ({contact.ContactName})\n" +
                                  $"🆔: /user_{contact.ContactUser.UniqueId}\n";

                sb.Append(contactInfo);
            }

            bool hasNextPage = contacts.Count >= pageSize;
            var replyMarkup = ReplyMarkupFactory.GetContactsListInlineKeyboard(hasNextPage ? pageNumber + 1 : null, pageNumber > 1);

            var text = sb.ToString();

            await _botClient.SendTextMessageAsync(chatId: userId,
                                                  text: text,
                                                  replyMarkup: replyMarkup);
        }

        private async Task SendDirectMessage(CallbackQuery callbackQuery)
        {
            var userId = callbackQuery.From.Id;

            if (long.TryParse(StringUtils.InlineKeyboardCallbackData.GetSendDirectMessageTargetUserId(callbackQuery.Data), out long targetUserId))
            {
                _userService.ChangeUserState(userId, UserState.SendingDirectMessage);
                _directMessageService.AddDirectMessageRequest(userId, targetUserId);

                var replyMarkup = ReplyMarkupFactory.GetInOperationKeyboard();

                var text = "Please enter your message to send to the user 👇";

                await _botClient.SendTextMessageAsync(chatId: userId,
                                                      text: text,
                                                      replyMarkup: replyMarkup);
            }
        }
    }
}