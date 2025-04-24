using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Tournament.Infrastructure.Logging
{
     public static class SeriLogConfiguration
    {
            public static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
            {
            var logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration) 
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();

            Log.Logger = logger;

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger, dispose: true);
            });
        }
    }
}
