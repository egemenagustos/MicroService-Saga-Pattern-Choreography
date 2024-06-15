using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Consumers;
using Order.Api.Contexts;
using Order.Api.Models;
using Order.Api.ViewModels;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentComletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();

    configurator.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq"]);

        config.ReceiveEndpoint(RabbitMqSettings.Order_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentComletedEventConsumer>(context));

        config.ReceiveEndpoint(RabbitMqSettings.Order_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));

        config.ReceiveEndpoint(RabbitMqSettings.Order_StockNotReservedEventQueue, e => e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
    });
});

builder.Services.AddDbContext<OrderApiDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["MsSql"]);
});

var app = builder.Build();

app.MapPost("/createOrder", async (CreateOrder model, OrderApiDbContext context, IPublishEndpoint publishEndpoint) =>
{
    Order.Api.Models.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        OrderItems = model.CreateOrderItems.Select(oi => new OrderItem()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = Guid.Parse(oi.ProductId)
        }).ToList(),
        OrderStatus = Order.Api.Enums.OrderStatus.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.CreateOrderItems.Sum(oi => oi.Price * oi.Count)
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();
    OrderCreatedEvent createdEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage()
        {
            Count = oi.Count,
            Price= oi.Price,
            ProductId = oi.ProductId
        }).ToList(),
        TotalPrice = order.TotalPrice
    };
    await publishEndpoint.Publish(createdEvent);
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
