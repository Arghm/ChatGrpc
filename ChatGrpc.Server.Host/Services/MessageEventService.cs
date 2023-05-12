using gRPCServiceApp.Protos;

namespace ChatGrpc.Server.Host.Services
{
    public class MessageEventService
    {
        public delegate Task SendMessage(StreamMessage message);
        public event SendMessage Broadcast;

        public Task BroadcastInvoke(StreamMessage message)
        {
            if (Broadcast != null)
                return Broadcast.Invoke(message);
            else
                return Task.CompletedTask;
        }
    }
}
