using System;
using Telegram.Bot.Types.Enums;

namespace NotSoBoring.WebHook.Services.Handlers.MessageTypeStrategies
{
    public static class MessageHandlerFactory
    {
        public static IMessageHandlerStrategy GetMessageHandler(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Text => new TextMessageHandler(),
                MessageType.Sticker => new StickerMessageHandler(),
                MessageType.Voice => new VoiceMessageHandler(),
                _ => new NoActionMessageHandler()
            };
        }
    }
}