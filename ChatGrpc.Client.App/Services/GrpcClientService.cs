using Grpc.Core;
using Grpc.Net.Client;
using ChatGrpc.Client.App.Services;
using ChatGrpcClient.Entities;
using gRPCServiceApp.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGrpcClient.Services
{
    public class GrpcClientService : Chatter.ChatterClient, IAsyncDisposable
    {
        private readonly ILogger<GrpcClientService> _logger;
        private readonly GrpcClientSettings _config;
        private readonly IIncomingMessageProcessService _incomingMessageService;

        private bool _disposed = false;

        private GrpcChannel _channel;
        private bool IsGrpcChannelReady => _channel != null && _channel.State == ConnectivityState.Ready;
        private Chatter.ChatterClient client;

        private AsyncDuplexStreamingCall<StreamMessage, StreamMessage> asyncDuplexStreamingCall;

        private bool isReponseReaderInitialized = false;

        public GrpcClientService(
            IOptions<GrpcClientSettings> options,
            IIncomingMessageProcessService incomingMessageService,
            ILogger<GrpcClientService> logger)
        {
            _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _incomingMessageService = incomingMessageService ?? throw new ArgumentNullException(nameof(incomingMessageService));
        }

        public Task Start(string userName)
        {
            // Обработка ответов вынесена в отдельный поток
            if (isReponseReaderInitialized)
                return Task.CompletedTask;
            client = GetChatterClient();
            asyncDuplexStreamingCall = client.MessengerService();

            Task readTask = Task.Run(async () =>
            {
                await foreach (var respMessage in asyncDuplexStreamingCall.ResponseStream.ReadAllAsync())
                {
                    await _incomingMessageService.ProcessIncomingMessage(FormatOutputMessage(respMessage));
                }
            });
            isReponseReaderInitialized = true;
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            await _incomingMessageService.ProcessIncomingMessage("Client destroyed");
            await asyncDuplexStreamingCall.RequestStream.CompleteAsync();
            _channel?.Dispose();
            _disposed = true;
        }

        private string FormatOutputMessage(StreamMessage message)
        {
            return $"{message.Username}: {message.Content}";
        }

        public async Task SendMessage(string userName, string message)
        {
            StreamMessage reqMessage = new StreamMessage { Username = userName, Content = message };
            await asyncDuplexStreamingCall.RequestStream.WriteAsync(reqMessage);
        }

        private GrpcChannel GetGrpcChannel()
        {
            if (IsGrpcChannelReady)
                return _channel;
            _channel = GrpcChannel.ForAddress(_config.GrpcChannel);
            return _channel;
        }

        private Chatter.ChatterClient GetChatterClient()
        {
            if (client != null)
            {
                if (IsGrpcChannelReady)
                {
                    return client;
                }
            }

            client = new Chatter.ChatterClient(GetGrpcChannel());
            return client;
        }

        private AsyncDuplexStreamingCall<StreamMessage, StreamMessage> GetAsyncDuplexStreamingCall()
        {
            return asyncDuplexStreamingCall;
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
