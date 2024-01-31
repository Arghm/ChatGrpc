using ChatGrpc.Client.App.Entities;
using ChatGrpc.Client.App.Services;
using ChatGrpc.Client.ConsoleHost.Services;
using ChatGrpcClient.Entities;
using ChatGrpcClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatGrpc.Client.Host
{
    public static class IoC
    {
        public static IServiceProvider Services { get; private set; }

        public static void ConfigureServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();

            // Adding my own service to the service collection
            services.AddSingleton<ChatGrpcClientService>();
            services.AddSingleton<IIncomingMessageProcessService, ConsoleProcessMessageService>();

            // Make strongly typed configuration available
            services.Configure<GrpcClientSettings>(configuration.GetSection("GrpcClientSettings"));
            services.Configure<ConsolePrefs>(configuration.GetSection("ConsolePrefs"));

            // Add required services for strongly typed configuration and logging
            services.AddOptions();
            services.AddLogging();

            Services = services.BuildServiceProvider();
        }
    }
}
