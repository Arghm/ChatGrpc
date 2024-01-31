using ChatGrpc.Client.App.Contracts;
using ChatGrpc.Client.App.Entities;
using ChatGrpc.Client.ConsoleHost.Entities;
using ChatGrpcClient.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChatGrpc.Client.Host
{
    internal class Program
    {
        static string _userName = string.Empty;
        static ConsoleSettings _consoleConfig;
        static GrpcClientSettings _clientConfig;
        private static IChatterClient _chatterClient;

        public static async Task Main(string[] args)
        {
            var configuration = Configure();
            IoC.ConfigureServices(configuration);
            ConfigureLogger();

            // get configs
            _consoleConfig = IoC.Services.GetService<IOptions<ConsoleSettings>>()?.Value ?? throw new ArgumentNullException(nameof(IOptions<ConsoleSettings>));
            _clientConfig = IoC.Services.GetService<IOptions<GrpcClientSettings>>()?.Value ?? throw new ArgumentNullException(nameof(IOptions<GrpcClientSettings>));
            _chatterClient = IoC.Services.GetService<IChatterClient>() ?? throw new ArgumentNullException(nameof(IChatterClient));

            Console.Title = _consoleConfig.Title;

            await ConnectToServer();
            _userName = await RegisterUser();
            await _chatterClient.Start(_userName);

            ChangeConsoleTitle();

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

                    await _chatterClient.SendMessage(_userName, inputText);
                }
            }
            await _chatterClient.Stop();

            Console.WriteLine("Shutdown...");
        }

        public static IConfiguration Configure()
        {
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

        /// <summary>
        /// Initialize connection
        /// </summary>
        private static async Task ConnectToServer()
        {
            while (!_chatterClient.TryInitialize())
            {
                Console.WriteLine($"Connection failed. Retry in {_clientConfig.ConnectionRetryTimeSeconds} sec");
                await Task.Delay(_clientConfig.ConnectionRetryTimeSeconds * 1000);
            }

            Console.WriteLine("Connected to the server");
        }

        /// <summary>
        /// Register User.
        /// </summary>
        private static async Task<string> RegisterUser()
        {
            string userName = string.Empty;
            var result = new RegisterUserResult { IsSuccess = false };
            while (!result.IsSuccess)
            {
                userName = GetUserName();
                result = await _chatterClient.RegisterUser(userName);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.RegisterMessage);
                }
            }
            return userName;
        }

        private static void ChangeConsoleTitle()
        {
            string newTitle = "Chat: ";

            if (!string.IsNullOrWhiteSpace(_consoleConfig?.Title))
                newTitle = _consoleConfig.Title;

            if (!string.IsNullOrWhiteSpace(_userName))
                newTitle += _userName;

            Console.Title = newTitle;
        }

        private static string GetUserName() 
        {
            string userName = string.Empty;
            while (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Enter user name:");
                userName = Console.ReadLine();
            }
            return userName;
        }
    }
}