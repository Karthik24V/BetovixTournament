
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Tournament.Api.Middlewares;
using Tournament.Domain.DataBase.DBContext;
using Tournament.Infrastructure.Logging;

namespace Tournament.Api.Extension
{
    public static class ServicesExtension
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSerilogLogging(configuration);
            services.AddAutoMapper(typeof(Tournament.Business.Mapper.AutoMapper));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("connectionString"));
            });

            services.AddMassTransit(x =>
            {
                var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMqConfig["Host"], h =>
                    {
                        h.Username(rabbitMqConfig["Username"]);
                        h.Password(rabbitMqConfig["Password"]);
                    });
                });
            });
        }

        public static void AddMiddlewareServices(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
