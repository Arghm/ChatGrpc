using ChatGrpc.Client.App.Contracts;
using ChatGrpc.Client.App.Services;
using ChatGrpcClient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatGrpc.Client.App.IoC
{
    public static class ConfigureServicesExtension
    {
        /// <summary>
        /// Register all Chatter Services.
        /// </summary>
        /// <typeparam name="T">Implementation of process message service : IIncomingMessageProcessService</typeparam>
        public static IServiceCollection ConfigureChatterServices<T>(this IServiceCollection services) where T: class, IIncomingMessageProcessService
        {
            services.AddSingleton<ChatGrpcClientService>();
            services.AddSingleton<IChatterClient, ChatterClient>();
            services.AddSingleton<IIncomingMessageProcessService, T>();
            return services;
        }
    }
}
