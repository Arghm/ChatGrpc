using Google.Protobuf;
using Grpc.Core;
using ChatGrpc.Server.Host;
using ChatGrpc.Server.Host.Entities;
using ChatGrpcServiceApp;

namespace ChatGrpc.Server.Host.Services;

public class ChatterService : Chatter.ChatterBase
{
    const string _serverName = "Server";

    private readonly ILogger<ChatterService> _logger;
    private readonly IProcessMessageService _messageService;
    private readonly IUserService _userService;

    private IServerStreamWriter<StreamIncMessage> _responseStream;
    private bool _isResponseReady = false;

    private string _userName;
    public ChatterService(
        ILogger<ChatterService> logger,
        IProcessMessageService messageService,
        IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _userService = userService ?? throw new ArgumentNullException(nameof(messageService));
    }

    public override async Task MessangerProcess(IAsyncStreamReader<StreamOutMessage> requestStream,
    IServerStreamWriter<StreamIncMessage> responseStream,
    ServerCallContext context)
    {
        // считываем вход€щие сообщени€ в фоновой задаче
        var readTask = Task.Run(async () =>
        {
            await foreach (StreamOutMessage message in requestStream.ReadAllAsync())
            {
                await ProcessMessage(message);
            }
        });

        UserJoinChat();

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
            await UserLeftChat();
        }
    }

    public override async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest registerUserRequest, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(registerUserRequest.Username))
            return new RegisterUserResponse
            {
                IsSuccess = false,
                Username = registerUserRequest.Username,
                RegisterMessage = "Name cannot be empty",
            };

        return MapToRegisterResponse(registerUserRequest.Username, _userService.TryRegisterUserName(registerUserRequest.Username));
    }

    public async Task SendResponseToClient(StreamIncMessage message)
    {
        if (_isResponseReady &&
            _responseStream != null)
        {
            await _responseStream.WriteAsync(message);
        }
    }

    private Task ProcessMessage(StreamOutMessage message)
    {
        if (!ValidateMessage(message))
        {
            return Task.CompletedTask;
        }

        if (message.IsServerMessage)
        {
            _userName = message.Username;
        }

        return _messageService.ProcessClientMessage(message);
    }

    private Task UserLeftChat()
    {
        //_messageEventService.Broadcast -= SendResponseToClient;
        _messageService.UnsubscribeUser(SendResponseToClient);
        UnregisterUser();

        return _messageService.ProcessClientMessage(new StreamOutMessage
        {
            Username = _serverName,
            Content = $"{_userName} left chat",
            IsServerMessage = true,
        });
    }

    private void UserJoinChat()
    {
        //_messageEventService.Broadcast += SendResponseToClient;
        _messageService.SubscribeUser(SendResponseToClient);
    }

    private void UnregisterUser()
    {
        _userService.UnregisterUser(_userName);
    }

    private bool ValidateMessage(StreamOutMessage message)
    {
        return _userService.ValidateToken(message.Username, message.Token);
    }

    private RegisterUserResponse MapToRegisterResponse(string userName, RegisterResult result)
    {
        return new RegisterUserResponse
        {
            IsSuccess = result.IsSuccess,
            RegisterMessage = result.Message,
            Username = userName,
            Token = result.Token,
        };
    }
}
