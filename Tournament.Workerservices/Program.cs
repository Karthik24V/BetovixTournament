using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tournament.Data.IRepository;
using Tournament.Data.Repository;
using Tournament.Domain.DataBase.DBContext;
using Tournament.Workerservices;
using Tournament.Workerservices.Consumers;
using Tournament.Workerservices.IWokerRepo;
using Tournament.Workerservices.IWorkerServices;
using Tournament.Workerservices.WorkerRepo;
using Tournament.Workerservices.WorkerServices;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString"));
});
builder.Services.AddScoped<IEventService,EventService>();
builder.Services.AddScoped<IEventRepo, EventRepo>();
builder.Services.AddAutoMapper(typeof(Tournament.Business.Mapper.AutoMapper));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TournamentConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConfig["Host"], h =>
        {
            h.Username(rabbitMqConfig["Username"]);
            h.Password(rabbitMqConfig["Password"]);
        });

        cfg.ReceiveEndpoint("tournament-queue", e =>
        {
            e.ConfigureConsumer<TournamentConsumer>(context);
        });
    });
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
