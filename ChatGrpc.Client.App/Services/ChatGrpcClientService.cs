using Grpc.Core;
using Grpc.Net.Client;
using ChatGrpc.Client.App.Services;
using ChatGrpcClient.Entities;
using ChatGrpcServiceApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ChatGrpc.Client.App.Entities;
using Microsoft.Win32;

namespace ChatGrpcClient.Services
{
    /// <summary>
    /// Chat grpc client.
    /// </summary>
    public class ChatGrpcClientService : Chatter.ChatterClient, IAsyncDisposable
    {
        private readonly ILogger<ChatGrpcClientService> _logger;
        private readonly GrpcClientSettings _config;
        private readonly IIncomingMessageProcessService _incomingMessageService;

        private bool _disposed = false;
        private GrpcChannel _channel;
        private bool _isGrpcChannelReady => _channel != null && _channel.State == ConnectivityState.Ready;
        private Chatter.ChatterClient _client;
        private AsyncDuplexStreamingCall<StreamOutMessage, StreamIncMessage> _asyncDuplexStreamingCall;
        private bool _isReponseReaderInitialized = false;
        private string _token;

        public bool IsReaderInitialized => _isReponseReaderInitialized;

        public ChatGrpcClientService(
            IOptions<GrpcClientSettings> options,
            IIncomingMessageProcessService incomingMessageService,
            ILogger<ChatGrpcClientService> logger)
        {
            _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _incomingMessageService = incomingMessageService ?? throw new ArgumentNullException(nameof(incomingMessageService));
        }

        /// <summary>
        /// Initialize connection for user.
        /// </summary>
        public InitializeResult Initialize()
        {
            // Обработка ответов вынесена в отдельный поток
            if (_isReponseReaderInitialized)
                return new InitializeResult { IsSuccess = true };

            _client = GetChatterClient();
            _asyncDuplexStreamingCall = _client.MessangerProcess();

            return new InitializeResult { IsSuccess = true };
        }

        /// <summary>
        /// Register user in chat.
        /// </summary>
        public async Task<RegisterUserResponse> RegisterUser(string userName)
        {
            _client = GetChatterClient();

            if (string.IsNullOrWhiteSpace(userName))
                return new RegisterUserResponse { IsSuccess = false, RegisterMessage = "Name cannot be empty" };

            var register = await _client.RegisterUserAsync(new RegisterUserRequest { Username = userName });
            _token = register.Token;

            return register;
        }

        public Task Start(string userName)
        {
            Task readTask = Task.Run(async () =>
            {
                await foreach (var respMessage in _asyncDuplexStreamingCall.ResponseStream.ReadAllAsync())
                {
                    await _incomingMessageService.ProcessIncomingMessage(respMessage.Content);
                }
            });
            _isReponseReaderInitialized = true;

            return SendMessage(userName, $"{userName} joined chat", true);
        }

        /// <summary>
        /// Use Stop or Dispose.
        /// </summary>
        public async Task Stop()
        {
            await _incomingMessageService.ProcessIncomingMessage("Client destroyed");
            await _asyncDuplexStreamingCall.RequestStream.CompleteAsync();
            _channel?.Dispose();
            _disposed = true;
        }

        public Task SendMessage(string userName, string message)
        {
            return SendMessage(userName, message, false);
        }

        public Task SendMessage(string userName, string message, bool isServerMessage)
        {
            StreamOutMessage reqMessage = new StreamOutMessage { Username = userName, Content = message, Token = _token, IsServerMessage = isServerMessage };
            return _asyncDuplexStreamingCall.RequestStream.WriteAsync(reqMessage);
        }

        private GrpcChannel GetGrpcChannel()
        {
            if (_isGrpcChannelReady)
                return _channel;
            _channel = GrpcChannel.ForAddress(_config.GrpcChannel);
            return _channel;
        }

        private Chatter.ChatterClient GetChatterClient()
        {
            if (_client != null)
            {
                if (_isGrpcChannelReady)
                {
                    return _client;
                }
            }

            _client = new Chatter.ChatterClient(GetGrpcChannel());
            return _client;
        }

        private AsyncDuplexStreamingCall<StreamOutMessage, StreamIncMessage> GetAsyncDuplexStreamingCall()
        {
            return _asyncDuplexStreamingCall;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if(!_disposed)
            {
                await Stop();
            }
        }
    }
}
