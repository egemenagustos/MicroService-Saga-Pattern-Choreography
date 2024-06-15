using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq"]);
    });
});

var app = builder.Build();

app.Run();
