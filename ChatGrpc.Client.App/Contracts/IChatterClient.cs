using ChatGrpc.Client.App.Entities;

namespace ChatGrpc.Client.App.Contracts
{
    /// <summary>
    /// Chatter client.
    /// </summary>
    public interface IChatterClient
    {
        /// <summary>
        /// Initialize connection.
        /// </summary>
        /// <returns>True when success</returns>
        bool TryInitialize();

        /// <summary>
        /// Try register new user.
        /// </summary>
        /// <param name="userName"></param>
        Task<RegisterUserResult> RegisterUser(string userName);

        /// <summary>
        /// Start messaging.
        /// </summary>
        /// <param name="userName"></param>
        Task Start(string userName);

        /// <summary>
        /// Stop service, close connections.
        /// </summary>
        Task Stop();

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <param name="isServerMessage">True: send message as "Server"</param>
        Task SendMessage(string userName, string message, bool isServerMessage = false);
    }
}
