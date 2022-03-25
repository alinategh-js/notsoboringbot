using NotSoBoring.Domain.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public class NoActionMessageHandler : IMessageHandlerStrategy
    {
        public Func<Task<Message>> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider)
        {
            return () => Task.FromResult(message);
        }
    }
}
