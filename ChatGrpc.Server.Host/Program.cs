using ChatGrpc.Server.Host.Services;
using Serilog;

namespace ChatGrpc.Server.Host;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigureLogger();

        Log.Information("Starting service");
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();
        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddTransient<IProcessMessageService, ProcessMessageService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<MessageEventService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<ChatterService>();
        app.MapGet("/", () => "Chatter server");

        app.Run();
    }

    private static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .WriteTo.File("Logs/log-.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}