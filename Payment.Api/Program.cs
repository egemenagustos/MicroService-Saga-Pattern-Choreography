using MassTransit;
using Payment.Api.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<StockReservedEventConsumer>();

    configurator.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq"]);

        config.ReceiveEndpoint(RabbitMqSettings.Payment_StockReservedEventQueue, e => e.ConfigureConsumer<StockReservedEventConsumer>(context));
    });
});

var app = builder.Build();

app.Run();
