using ChatGrpc.Server.Host.Entities;
using static ChatGrpc.Server.Host.Services.MessageEventService;

namespace ChatGrpc.Server.Host.Services
{
    /// <summary>
    /// User service.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Try to register new user name.
        /// </summary>
        /// <returns>true is success</returns>
        RegisterResult TryRegisterUserName(string userName);

        /// <summary>
        /// Validate user by token.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool ValidateToken(string userName, string userToken);

        /// <summary>
        /// Unregister user and unsubscribe from braodcast event.
        /// </summary>
        /// <param name="userName"></param>
        void UnregisterUser(string userName);
    }
}
