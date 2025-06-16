using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Tournament.Api.Extension;
using Tournament.Common.DTOs;

namespace Tournament.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services extra container.
            builder.Services.AddApplicationServices(builder.Configuration);
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddScoped<IValidator<CreateTournamentDto>, CreateTournamentDtoValidator>();
            builder.Services.AddScoped<IValidator<ParticipationDto>, ParticipationDtoValidator>();
            builder.Services.AddScoped<IValidator<TournamentParticipationRuleDto>, TournamentParticipationRuleDtoValidator>();
            builder.Services.AddScoped<IValidator<UpdateTournamentDto>, UpdateTournamentDtoValidator>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Tournament API", Version = "v1" });
                // JWT Bearer token setup
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                // API Key setup
                c.AddSecurityDefinition("API-KEY", new OpenApiSecurityScheme
                {
                    Description = "API Key needed to access this endpoint.",
                    Name = "X-API-KEY", 
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "API-KEY"
                });
                          
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                      {
                        {new OpenApiSecurityScheme{ Reference = new OpenApiReference 
                                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},new string[] {}},
                        {new OpenApiSecurityScheme { Reference = new OpenApiReference
                                { Type = ReferenceType.SecurityScheme, Id = "API-KEY" }}, new string[] {}}
                       });
                 });
            var app = builder.Build();

            //added the custom middleware
            app.AddMiddlewareServices();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tournament API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
