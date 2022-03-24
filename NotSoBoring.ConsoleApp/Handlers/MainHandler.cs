using System;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Microsoft.Extensions.Configuration;
using NotSoBoring.ConsoleApp.Models;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

namespace NotSoBoring.ConsoleApp.Handlers
{
    public class MainHandler
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IConfiguration _configs;

        public MainHandler(CancellationTokenSource cancellationTokenSource, IConfiguration configs)
        {
            _cancellationToken = cancellationTokenSource.Token;
            _configs = configs;
        }

        public async Task Start()
        {
            var botToken = _configs["TelegramBot:Token"];
            var botClient = new TelegramBotClient(botToken);

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions,
                cancellationToken: _cancellationToken);
            
            //botClient.SetMyCommandsAsync();
            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var commands = BotCommands.Commands;
            
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Type != UpdateType.Message)
                return;
            // Only process text messages
            if (update.Message!.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
