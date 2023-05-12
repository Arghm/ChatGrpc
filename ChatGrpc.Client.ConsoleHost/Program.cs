using ChatGrpcClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Threading.Tasks;

namespace ChatGrpc.Client.Host
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = Configure();
            IoC.ConfigureServices(configuration);
            ConfigureLogger();

            Console.WriteLine("Enter user name:");
            string userName = Console.ReadLine() ?? "anon";

            string prefix = userName + ": ";
            await using (GrpcClientService service = IoC.Services.GetService<GrpcClientService>() ?? throw new ArgumentNullException(nameof(GrpcClientService)))
            {
                await service.Start(userName);

                Console.WriteLine("For exit type '\\Q' or '\\q'");
                string? inputText;
                while (true)
                {
                    inputText = Console.ReadLine();
                    //place cursor on the beginning of line to rewrite message by server answer
                    Console.SetCursorPosition(0, Console.CursorTop - 1); 
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        if (inputText.ToUpper() == "\\Q")
                        {
                            break;
                        }

                        await service.SendMessage(userName, inputText);
                    }
                }
                //await service.Stop();
            }
            Console.WriteLine("Shutdown...");
        }

        public static IConfiguration Configure()
        {
            var t = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("Logs/log-.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var loggerFactory = IoC.Services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddSerilog();
        }
    }
}