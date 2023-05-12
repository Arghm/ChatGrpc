using ChatGrpc.Server.Host.Entities;
using gRPCServiceApp.Protos;

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
        Task ProcessClientMessage(StreamMessage message);
    }
}
