using NotSoBoring.Domain.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public class NoActionMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            return () => Task.CompletedTask;
        }
    }
}
