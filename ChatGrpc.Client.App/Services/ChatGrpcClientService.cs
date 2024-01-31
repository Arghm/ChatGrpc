using Grpc.Core;
using Grpc.Net.Client;
using ChatGrpc.Client.App.Services;
using ChatGrpcClient.Entities;
using ChatGrpcServiceApp;
using Microsoft.Extensions.Options;

namespace ChatGrpcClient.Services
{
    /// <summary>
    /// Chat grpc client.
    /// </summary>
    public class ChatGrpcClientService : Chatter.ChatterClient, IAsyncDisposable
    {
        private readonly GrpcClientSettings _config;
        private readonly IIncomingMessageProcessService _incomingMessageService;

        private bool _disposed = false;
        private GrpcChannel _channel;
        private bool _isGrpcChannelReady => _channel != null && _channel.State == ConnectivityState.Ready;
        private Chatter.ChatterClient _client;
        private AsyncDuplexStreamingCall<StreamOutMessage, StreamIncMessage> _asyncDuplexStreamingCall;
        private bool _isReponseReaderInitialized = false;

        public bool IsReaderInitialized => _isReponseReaderInitialized;

        public ChatGrpcClientService(
            IOptions<GrpcClientSettings> options,
            IIncomingMessageProcessService incomingMessageService)
        {
            _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _incomingMessageService = incomingMessageService ?? throw new ArgumentNullException(nameof(incomingMessageService));
        }

        /// <summary>
        /// Initialize connection for user
        /// </summary>
        /// <returns>True if success</returns>
        public bool TryInitialize()
        {
            // Обработка ответов вынесена в отдельный поток
            if (_isReponseReaderInitialized)
                return true;

            _client = GetChatterClient();
            _asyncDuplexStreamingCall = _client.MessangerProcess();
            _isReponseReaderInitialized = true;
            return true;
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

            return register;
        }

        public void Start()
        {
            Task readTask = Task.Run(async () =>
            {
                await foreach (var respMessage in _asyncDuplexStreamingCall.ResponseStream.ReadAllAsync())
                {
                    await _incomingMessageService.ProcessIncomingMessage(respMessage.Content);
                }
            });
            _isReponseReaderInitialized = true;
        }

        /// <summary>
        /// Use Stop or Dispose.
        /// </summary>
        public async Task Stop()
        {
            await _asyncDuplexStreamingCall.RequestStream.CompleteAsync();
            _isReponseReaderInitialized = false;
            _channel?.Dispose();
            _disposed = true;
        }

        public Task SendMessage(StreamOutMessage outMessage)
        {
            return _asyncDuplexStreamingCall.RequestStream.WriteAsync(outMessage);
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
