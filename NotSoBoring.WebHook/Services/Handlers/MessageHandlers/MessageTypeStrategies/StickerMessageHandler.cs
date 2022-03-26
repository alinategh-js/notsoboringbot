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
            else if (userState > UserState.Edit_Profile_Start && userState < UserState.Edit_Profile_End) // user is in edit mode
            {
                action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().InvalidInput(message);
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}