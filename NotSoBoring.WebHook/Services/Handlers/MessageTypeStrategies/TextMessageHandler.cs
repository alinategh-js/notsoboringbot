using System;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public class TextMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task<Message>> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            Func<Task<Message>> action = null;

            if (userState == UserState.InMenu || userState == UserState.WaitingForMatch)
            {
                action = message.Text! switch
                {
                    StringUtils.Strings.ConnectMeToAnAnonymous or "/connect" => async () => await serviceProvider.GetRequiredService<CommandHandler>().ConnectToAnonymous(message),
                    StringUtils.Strings.CancelRequest or "/cancel" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelRequest(message),
                    StringUtils.Strings.CancelSession or "/endsession" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelSession(message),
                    _ => async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message)
                };
            }
            else if (userState == UserState.InSession)
            {
                action = message.Text! switch
                {
                    StringUtils.Strings.CancelSession or "/endsession" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelSession(message),
                    _ => async () => await serviceProvider.GetRequiredService<SessionHandler>().SendSessionTextMessage(message)
                };
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}