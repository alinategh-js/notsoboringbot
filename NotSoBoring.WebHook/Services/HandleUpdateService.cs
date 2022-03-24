using Microsoft.Extensions.Logging;
using NotSoBoring.Domain.DTOs;
using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Matchmaking;
using NotSoBoring.Matchmaking.Users;
using NotSoBoring.WebHook.Services.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotSoBoring.WebHook.Services
{
    public class HandleUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;
        private readonly UserService _userService;
        private readonly CommandHandler _commandHandler;
        private readonly SessionHandler _sessionHandler;
        private readonly MatchingEngine _matchingEngine;

        public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger,
            UserService userService, CommandHandler commandHandler, MatchingEngine matchingEngine, SessionHandler sessionHandler)
        {
            _botClient = botClient;
            _logger = logger;
            _userService = userService;
            _commandHandler = commandHandler;
            _sessionHandler = sessionHandler;
            _matchingEngine = matchingEngine;
        }

        public async Task EchoAsync(Update update)
        {
            Func<Task> handler = update.Type switch
            {
                UpdateType.Message => async () => await BotOnMessageReceived(update.Message!),
                //UpdateType.EditedMessage => async () => await BotOnMessageReceived(update.EditedMessage!),
                //UpdateType.CallbackQuery => async () => await BotOnCallbackQueryReceived(update.CallbackQuery!),
                //UpdateType.InlineQuery => async () => await BotOnInlineQueryReceived(update.InlineQuery!),
                //UpdateType.ChosenInlineResult => async () => await BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
                _ => async () => await UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler.Invoke();
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            _logger.LogInformation("Receive message type: {messageType}", message.Type);
            if (message.Type != MessageType.Text)
                return;

            #region Check if user exists
            var user = await _userService.GetUser(message.From.Id);
            if (user == null)
            {
                // register user
                await _userService.AddUser(message.From.Id);
            }
            #endregion

            #region Get User's state
            var userState = _matchingEngine.GetUserState(message.From.Id);
            #endregion

            Task<Message> action = null;

            #region Determine action from user's state
            if (userState == UserState.InMenu || userState == UserState.WaitingForMatch)
            {
                action = message.Text! switch
                {
                    StringUtils.Strings.ConnectMeToAnAnnonymous or "/connect" => _commandHandler.ConnectToAnonymous(_botClient, message),
                    StringUtils.Strings.CancelRequest or "/cancel" => _commandHandler.CancelRequest(_botClient, message),
                    StringUtils.Strings.CancelSession or "/endsession" => _commandHandler.CancelSession(_botClient, message),
                    _ => Usage(_botClient, message)
                };
            }
            else if (userState == UserState.InSession)
            {
                action = message.Text! switch
                {
                    StringUtils.Strings.CancelSession or "/endsession" => _commandHandler.CancelSession(_botClient, message),
                    _ => _sessionHandler.SendSessionTextMessage(_botClient, message)
                };
            }
            else action = Usage(_botClient, message);
            #endregion

            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {sentMessageId}", sentMessage.MessageId);

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            //static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
            //{
            //    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            //    // Simulate longer running task
            //    await Task.Delay(500);

            //    InlineKeyboardMarkup inlineKeyboard = new(
            //        new[]
            //        {
            //        // first row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("1.1", "11"),
            //            InlineKeyboardButton.WithCallbackData("1.2", "12"),
            //        },
            //        // second row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("2.1", "21"),
            //            InlineKeyboardButton.WithCallbackData("2.2", "22"),
            //        },
            //        });

            //    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                          text: "Choose",
            //                                          replyMarkup: inlineKeyboard);
            //}

            //static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
            //{
            //    ReplyKeyboardMarkup replyKeyboardMarkup = new(
            //        new[]
            //        {
            //            new KeyboardButton[] { "1.1", "1.2" },
            //            new KeyboardButton[] { "2.1", "2.2" },
            //        })
            //    {
            //        ResizeKeyboard = true
            //    };

            //    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                          text: "Choose",
            //                                          replyMarkup: replyKeyboardMarkup);
            //}

            //static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
            //{
            //    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                          text: "Removing keyboard",
            //                                          replyMarkup: new ReplyKeyboardRemove());
            //}

            //static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
            //{
            //    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            //    const string filePath = @"Files/tux.png";
            //    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            //    return await bot.SendPhotoAsync(chatId: message.Chat.Id,
            //                                    photo: new InputOnlineFile(fileStream, fileName),
            //                                    caption: "Nice Picture");
            //}

            //static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
            //{
            //    ReplyKeyboardMarkup RequestReplyKeyboard = new(
            //        new[]
            //        {
            //        KeyboardButton.WithRequestLocation("Location"),
            //        KeyboardButton.WithRequestContact("Contact"),
            //        });

            //    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                          text: "Who or Where are you?",
            //                                          replyMarkup: RequestReplyKeyboard);
            //}

            static async Task<Message> Usage(ITelegramBotClient bot, Message message)
            {
                const string usage = "نگرفتم چی گفتی 🤔\n\n" +
                                     "از منوی پایین میتونی انتخاب کنی تا کمکت کنم 👇";

                var replyMarkup = ReplyMarkupFactory.GetDefaultKeyboardReplyMarkup();
                
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyToMessageId: message.MessageId,
                                                      replyMarkup: replyMarkup);
            }
        }

        // Process Inline Keyboard callback data
        //private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        //{
        //    await _botClient.AnswerCallbackQueryAsync(
        //        callbackQueryId: callbackQuery.Id,
        //        text: $"Received {callbackQuery.Data}");

        //    await _botClient.SendTextMessageAsync(
        //        chatId: callbackQuery.Message.Chat.Id,
        //        text: $"Received {callbackQuery.Data}");
        //}

        //#region Inline Mode

        //private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        //{
        //    _logger.LogInformation("Received inline query from: {inlineQueryFromId}", inlineQuery.From.Id);

        //    InlineQueryResult[] results = {
        //    // displayed result
        //        new InlineQueryResultArticle(
        //            id: "3",
        //            title: "TgBots",
        //            inputMessageContent: new InputTextMessageContent(
        //                "hello"
        //            )
        //        )
        //    };

        //    await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
        //                                            results: results,
        //                                            isPersonal: true,
        //                                            cacheTime: 0);
        //}

        //private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        //{
        //    _logger.LogInformation("Received inline result: {chosenInlineResultId}", chosenInlineResult.ResultId);
        //    return Task.CompletedTask;
        //}

        //#endregion

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {updateType}", update.Type);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
