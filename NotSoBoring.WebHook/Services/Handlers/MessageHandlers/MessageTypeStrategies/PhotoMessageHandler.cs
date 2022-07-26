using System;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public class PhotoMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task> action = null;

            if (userState == UserState.InSession)
            {
                action = async () => await serviceProvider.GetRequiredService<SessionHandler>().SendPhotoMessage(message);
            }
            else if (userState > UserState.Edit_Profile_Start && userState < UserState.Edit_Profile_End) // user is in edit mode
            {
                action = userState switch
                {
                    UserState.EditingPhoto => async () => await serviceProvider.GetRequiredService<CommandHandler>().EditProfilePhoto(message),
                    _ => async () => await serviceProvider.GetRequiredService<GeneralHandler>().InvalidInput(message)
                };
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}