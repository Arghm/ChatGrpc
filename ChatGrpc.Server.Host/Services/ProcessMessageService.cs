using ChatGrpc.Server.Host.Entities;
using gRPCServiceApp.Protos;

namespace ChatGrpc.Server.Host.Services
{
    public class ProcessMessageService : IProcessMessageService
    {
        private readonly ILogger<ProcessMessageService> _logger;
        private readonly MessageEventService _messageEventService;
        public ProcessMessageService(ILogger<ProcessMessageService> logger, MessageEventService messageEventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageEventService = messageEventService ?? throw new ArgumentNullException(nameof(messageEventService));
        }

        public Task ProcessClientMessage(StreamMessage message)
        {
            Console.WriteLine($"{message.Username}: {message.Content}");

            return _messageEventService.BroadcastInvoke(message);
        }
    }
}
