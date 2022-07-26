using Microsoft.Extensions.DependencyInjection;
using NotSoBoring.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public class LocationMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task> action = null;

            if (userState == UserState.InSession)
            {
                action = async () => await serviceProvider.GetRequiredService<SessionHandler>().SendLocationMessage(message);
            }
            else if (userState > UserState.Edit_Profile_Start && userState < UserState.Edit_Profile_End) // user is in edit mode
            {
                action = userState switch
                {
                    UserState.EditingLocation => async () => await serviceProvider.GetRequiredService<CommandHandler>().EditLocation(message),
                    _ => async () => await serviceProvider.GetRequiredService<GeneralHandler>().InvalidInput(message)
                };
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}
