using Serilog;
using Serilog.Events;

namespace Realworlddotnet.Infrastructure.Extensions.Logging;

public static class SerilogConfigurationExtensions
{
    public static LoggerConfiguration ConfigureBaseLogging(
        this LoggerConfiguration loggerConfiguration,
        string appName)
    {
        loggerConfiguration.MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", appName)
            .WriteTo.Async(a =>
                a.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {UserId} {Message:lj}{NewLine}{Exception}"));
        return loggerConfiguration;
    }
    
}
