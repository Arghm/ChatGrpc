using ChatGrpc.Server.Host.Entities;
using ChatGrpcServiceApp;
using static ChatGrpc.Server.Host.Services.MessageEventService;

namespace ChatGrpc.Server.Host.Services
{
    public class ProcessMessageService : IProcessMessageService
    {
        private readonly ILogger<ProcessMessageService> _logger;
        private readonly MessageEventService _messageEventService;
        public ProcessMessageService(ILogger<ProcessMessageService> logger, 
            MessageEventService messageEventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventService = messageEventService ?? throw new ArgumentNullException(nameof(messageEventService));
        }

        /// <inheritdoc/>
        public Task ProcessClientMessage(StreamOutMessage message)
        {
            var incMessage = Map(message);
            Console.WriteLine(incMessage);
            return _messageEventService.BroadcastInvoke(incMessage);
        }

        /// <inheritdoc/>
        public void SubscribeUser(SendMessage sendMessage)
        {
            _messageEventService.Broadcast += sendMessage;
        }

        /// <inheritdoc/>
        public void UnsubscribeUser(SendMessage delegateSendMessage)
        {
            _messageEventService.UnsubscribeUser(delegateSendMessage);
        }

        private StreamIncMessage Map(StreamOutMessage source)
        {
            string senderName = source.IsServerMessage ? "Server" : source.Username;
            return new StreamIncMessage
            {
                IsServerMessage = source.IsServerMessage,
                Content = $"{senderName}: {source.Content}"
            };
        }

    }
}
