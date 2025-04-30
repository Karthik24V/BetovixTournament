
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Tournament.Api.Middlewares;
using Tournament.Common.Dto_s;
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
            services.Configure<ApiKeySettings>(configuration.GetSection("ApiKeySettings"));

            // added the DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("connectionString"));
            });

            // Add MassTransit and configure RabbitMQ
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

            //add authentication services
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, AuthenticationhandlerExtension>("API-KEY", null);
        }

        public static void AddMiddlewareServices(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
