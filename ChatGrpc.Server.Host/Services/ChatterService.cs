using Google.Protobuf;
using Grpc.Core;
using ChatGrpc.Server.Host;
using ChatGrpc.Server.Host.Entities;
using gRPCServiceApp.Protos;

namespace ChatGrpc.Server.Host.Services;

public class ChatterService : Chatter.ChatterBase
{
    const string _serverName = "Server";

    private readonly ILogger<ChatterService> _logger;
    private readonly IProcessMessageService _messageService;
    private readonly MessageEventService _messageEventService;

    private IServerStreamWriter<StreamMessage> _responseStream;
    private bool _isResponseReady = false;

    private string _userName;
    public ChatterService(
        ILogger<ChatterService> logger,
        IProcessMessageService messageService,
        MessageEventService messageEventService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _messageEventService = messageEventService ?? throw new ArgumentNullException(nameof(messageEventService));
    }

    public override async Task MessengerService(IAsyncStreamReader<StreamMessage> requestStream,
    IServerStreamWriter<StreamMessage> responseStream,
    ServerCallContext context)
    {
        // считываем вход€щие сообщени€ в фоновой задаче
        var readTask = Task.Run(async () =>
        {
            await foreach (StreamMessage message in requestStream.ReadAllAsync())
            {
                _userName = message.Username;
                await _messageService.ProcessClientMessage(message);
            }
        });

        _messageEventService.Broadcast += SendResponseToClient;

        _responseStream = responseStream;
        _isResponseReady = true;

        // ожидаем завершени€ задачи чтени€/отправки
        try
        {
            await readTask;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Request stream read error");
        }
        finally
        {
            _messageEventService.Broadcast -= SendResponseToClient;
            await _messageService.ProcessClientMessage(new StreamMessage { Username = _serverName, Content = $"{_userName} left chat" });
        }
    }

    public async Task SendResponseToClient(StreamMessage message)
    {
        if (_isResponseReady &&
            _responseStream != null)
        {
            await _responseStream.WriteAsync(message);
        }
    }
}
