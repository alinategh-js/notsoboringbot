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
                    string command when command.StartsWith("/user_") => async () => await serviceProvider.GetRequiredService<CommandHandler>().ShowProfile(message, command.Substring(6)),
                    _ => async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message)
                };
            }
            else if (userState == UserState.InSession)
            {
                action = message.Text! switch
                {
                    StringUtils.Keyboard.CancelSession or "/endsession" => async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelSession(message),
                    StringUtils.Keyboard.Profile or "/profile" => async () => await serviceProvider.GetRequiredService<CommandHandler>().ShowProfile(message),
                    StringUtils.Keyboard.SeeContactProfile => async () => await serviceProvider.GetRequiredService<CommandHandler>().ShowContactProfile(message),
                    string command when command.StartsWith("/user_") => async () => await serviceProvider.GetRequiredService<CommandHandler>().ShowProfile(message, command.Substring(6)),
                    _ => async () => await serviceProvider.GetRequiredService<SessionHandler>().SendTextMessage(message)
                };
            }
            else if (userState > UserState.Edit_Profile_Start && userState < UserState.Edit_Profile_End)
            {
                if(message.Text == StringUtils.Keyboard.CancelEdit)
                {
                    action = async () => await serviceProvider.GetRequiredService<CommandHandler>().CancelEditProfile(message);
                }
                else
                {
                    action = userState switch
                    {
                        UserState.EditingNickname => async () => await serviceProvider.GetRequiredService<CommandHandler>().EditNickname(message),
                        UserState.EditingAge => async () => await serviceProvider.GetRequiredService<CommandHandler>().EditAge(message),
                        _ => () => Task.CompletedTask
                    };
                }
            }
            else action = async () => await serviceProvider.GetRequiredService<GeneralHandler>().Usage(message);

            return action;
        }
    }
}