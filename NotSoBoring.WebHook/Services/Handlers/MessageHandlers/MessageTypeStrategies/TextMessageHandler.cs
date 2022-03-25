using System;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public class TextMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task> action = null;

            if (userState == UserState.InMenu || userState == UserState.WaitingForMatch)
            {
                action = message.Text! switch
                {
                    StringUtils.Keyboard.ConnectMeToAnAnonymous or "/connect" => async () => await serviceProvider.GetRequiredService<CommandHandler>().ConnectToAnonymous(message),
                    StringUtils.Keyboard.CancelRequest or "/cancel" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelRequest(message),
                    StringUtils.Keyboard.CancelSession or "/endsession" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelSession(message),
                    StringUtils.Keyboard.Profile or "/profile" => async () => await serviceProvider.GetRequiredService<CommandHandler>().ShowProfile(message),
                    _ => async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message)
                };
            }
            else if (userState == UserState.InSession)
            {
                action = message.Text! switch
                {
                    StringUtils.Keyboard.CancelSession or "/endsession" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelSession(message),
                    _ => async () => await serviceProvider.GetRequiredService<SessionHandler>().SendTextMessage(message)
                };
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}