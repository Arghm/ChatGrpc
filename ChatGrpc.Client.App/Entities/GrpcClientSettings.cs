namespace ChatGrpcClient.Entities
{
    public class GrpcClientSettings
    {
        public string GrpcChannel { get; set; }
        public int ConnectionRetryTimeSeconds { get; set; }
    }
}
