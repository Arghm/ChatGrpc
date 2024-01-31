using ChatGrpc.Client.App.IoC;
using ChatGrpc.Client.ConsoleHost.Entities;
using ChatGrpc.Client.ConsoleHost.Services;
using ChatGrpcClient.Entities;
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
            // Add required services for strongly typed configuration and logging
            services.AddOptions();
            services.AddLogging();

            // Make strongly typed configuration available
            services.Configure<GrpcClientSettings>(configuration.GetSection(nameof(GrpcClientSettings)));
            services.Configure<ConsoleSettings>(configuration.GetSection(nameof(ConsoleSettings)));

            // Adding ChatterClient services to the service collection
            services.ConfigureChatterServices<ConsoleProcessMessageService>();

            Services = services.BuildServiceProvider();
        }
    }
}
