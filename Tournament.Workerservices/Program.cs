using MassTransit;
using Tournament.Workerservices;
using Tournament.Workerservices.Consumers;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
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
