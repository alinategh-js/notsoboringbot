using System;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public class PhotoMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task<Message>> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task<Message>> action = null;

            if (userState == UserState.InSession)
            {
                action = async () => await serviceProvider.GetRequiredService<SessionHandler>().SendPhotoMessage(message);
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}