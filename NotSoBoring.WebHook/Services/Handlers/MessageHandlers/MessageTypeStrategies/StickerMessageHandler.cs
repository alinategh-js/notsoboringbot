using Microsoft.Extensions.DependencyInjection;
using NotSoBoring.Domain.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public class StickerMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task> action = null;

            if (userState == UserState.InSession)
            {
                action = async () => await serviceProvider.GetRequiredService<SessionHandler>().SendStickerMessage(message);
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}