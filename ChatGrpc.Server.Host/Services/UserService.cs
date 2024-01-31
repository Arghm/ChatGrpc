using ChatGrpc.Server.Host.Entities;
using System.Collections.Concurrent;

namespace ChatGrpc.Server.Host.Services
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        /// <inheritdoc/>
        public RegisterResult TryRegisterUserName(string userName)    
        {
            var token = Guid.NewGuid().ToString();
            if (_users.TryAdd(userName, token))
            {
                return new RegisterResult { IsSuccess = true, Message = "Registered", Token = token };
            }
            return new RegisterResult { IsSuccess = false, Message = $"Name '{userName}' is already registered.", Token = string.Empty };
        }

        /// <inheritdoc/>
        public void UnregisterUser(string userName)
        {
            try
            {
                _users.TryRemove(userName, out string _);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <inheritdoc/>
        public bool ValidateToken(string userName, string userToken)
        {
            if (_users.TryGetValue(userName, out string token))
            {
                return token == userToken;
            }
            return false;
        }
    }
}
