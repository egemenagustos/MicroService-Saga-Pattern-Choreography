using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.Api.Consumers;
using Stock.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();

    configurator.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq"]);

        config.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));

        config.ReceiveEndpoint(RabbitMqSettings.Stock_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();

var collection = mongoDbService.GetCollection<Stock.Api.Models.Stock>();

if(!collection.FindSync(x => true).Any())
{
    await collection.InsertOneAsync(new()
    {
        ProductId = Guid.NewGuid().ToString(),
        Count = 100,
    });

    await collection.InsertOneAsync(new()
    {
        ProductId = Guid.NewGuid().ToString(),
        Count = 200,
    });

    await collection.InsertOneAsync(new()
    {
        ProductId = Guid.NewGuid().ToString(),
        Count = 50,
    });

    await collection.InsertOneAsync(new()
    {
        ProductId = Guid.NewGuid().ToString(),
        Count = 5,
    });
}

app.Run();
