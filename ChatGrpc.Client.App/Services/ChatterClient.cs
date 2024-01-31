using ChatGrpc.Client.App.Contracts;
using ChatGrpc.Client.App.Entities;
using ChatGrpcClient.Services;
using ChatGrpcServiceApp;
using Microsoft.Extensions.Logging;

namespace ChatGrpc.Client.App.Services
{
    public class ChatterClient : IChatterClient
    {
        private readonly ILogger<ChatterClient> _logger;
        private readonly ChatGrpcClientService _clientService;

        private string _token;

        public ChatterClient(ILogger<ChatterClient> logger,
            ChatGrpcClientService clientService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        /// <inheritdoc/>
        public bool TryInitialize()
        {
            try
            {
                return _clientService.TryInitialize();
            }
            catch (Exception e)
            {
                _logger.LogError("Initialization error: {e}", e);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<RegisterUserResult> RegisterUser(string userName)
        {
            var result = await _clientService.RegisterUser(userName);
            if (result.IsSuccess)
            {
                _token = result.Token;
            }
            return Map(result);
        }

        /// <inheritdoc/>
        public Task Start(string userName)
        {
            _clientService.Start();
            return SendMessage(userName, $"{userName} joined chat", true);
        }

        /// <inheritdoc/>
        public Task Stop()
        {
            try
            {
                return _clientService.Stop();
            }
            catch(Exception e)
            {
                _logger.LogError("Stop error: {e}", e);
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public Task SendMessage(string userName, string message, bool isServerMessage = false)
        {
            Validate(userName, message);
            var outMessage = new StreamOutMessage
            {
                Username = userName,
                Token = _token,
                Content = message,
                IsServerMessage = isServerMessage,
            };
            try
            {
                return _clientService.SendMessage(outMessage);
            }
            catch(Exception e)
            {
                _logger.LogError("SendMessage error:{e}", e);
                return Task.CompletedTask;
            }
        }

        private void Validate(string userName, string message)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message));
            }
        }

        private RegisterUserResult Map(RegisterUserResponse result)
        {
            return new RegisterUserResult
            {
                IsSuccess = result.IsSuccess,
                RegisterMessage = result.RegisterMessage,
            };
        }
    }
}
