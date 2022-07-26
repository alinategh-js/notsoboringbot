using System;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using NotSoBoring.Domain.Enums;

namespace NotSoBoring.WebHook.Services.Handlers.MessageHandlers.MessageTypeStrategies
{
    public interface IMessageHandlerStrategy
    {
        public Func<Task> HandleMessage(Message message, UserState userState, IServiceProvider serviceProvider);
    }
}