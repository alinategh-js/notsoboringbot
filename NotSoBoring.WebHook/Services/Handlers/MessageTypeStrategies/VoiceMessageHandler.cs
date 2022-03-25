using Microsoft.Extensions.DependencyInjection;
using NotSoBoring.Domain.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public class VoiceMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task<Message>> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task<Message>> action = null;

            if (userState == UserState.InSession)
            {
                action = async () => await serviceProvider.GetRequiredService<SessionHandler>().SendSessionStickerMessage(message);
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}
