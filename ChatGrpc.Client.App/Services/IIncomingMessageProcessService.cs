namespace ChatGrpc.Client.App.Services
{
    /// <summary>
    /// Service for processing incoming messages (print/log etc).
    /// </summary>
    public interface IIncomingMessageProcessService
    {
        /// <summary>
        /// Process incoming message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task ProcessIncomingMessage(string message);
    }
}
