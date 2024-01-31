using ChatGrpc.Server.Host.Entities;
using ChatGrpcServiceApp;
using static ChatGrpc.Server.Host.Services.MessageEventService;

namespace ChatGrpc.Server.Host.Services
{
    /// <summary>
    /// Service for processing messages
    /// </summary>
    public interface IProcessMessageService
    {
        /// <summary>
        /// Process client message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Delivery result</returns>
        Task ProcessClientMessage(StreamOutMessage message);

        /// <summary>
        /// Subscribe for braodcast event.
        /// </summary>
        void SubscribeUser(SendMessage sendMessage);

        /// <summary>
        /// Unsubscribe from braodcast event.
        /// </summary>
        void UnsubscribeUser(SendMessage delegateSendMessage);
    }
}