using NotSoBoring.Domain.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public interface IMessageHandlerStrategy
    {
        public Func<Task<Message>> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider);
    }
}
