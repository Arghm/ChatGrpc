using ChatGrpcServiceApp;

namespace ChatGrpc.Server.Host.Services
{
    public class MessageEventService
    {
        public delegate Task SendMessage(StreamIncMessage message);
        public event SendMessage Broadcast;

        /// <summary>
        /// Broadcast message to all chat.
        /// </summary>
        /// <param name="message"></param>
        public Task BroadcastInvoke(StreamIncMessage message)
        {
            if (Broadcast != null)
                return Broadcast.Invoke(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Unsubscribe user and unsubscribe from braodcast event.
        /// </summary>
        /// <param name="delegateSendMessage"></param>
        public void UnsubscribeUser(SendMessage delegateSendMessage)
        {
            Broadcast -= delegateSendMessage;
        }
    }
}
