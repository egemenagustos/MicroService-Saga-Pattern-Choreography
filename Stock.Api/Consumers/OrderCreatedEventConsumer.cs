using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.Api.Services;

namespace Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();

            IMongoCollection<Models.Stock> mongoCollection = 
                mongoDbService.GetCollection<Models.Stock>();

            foreach(var orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await(await mongoCollection
                    .FindAsync(x => x.ProductId == orderItem.ProductId.ToString() && x.Count >= orderItem.Count))
                    .AnyAsync());
            }

            if(stockResult.TrueForAll(x => x.Equals(true)))
            {
                foreach(var item in context.Message.OrderItems)
                {
                    Models.Stock stock = await (await mongoCollection
                        .FindAsync(x => x.ProductId == item.ProductId.ToString())).FirstOrDefaultAsync();

                    stock.Count -= item.Count;

                    await mongoCollection.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId.ToString(), stock);
                }

                var sendEndpoint = await sendEndpointProvider
                    .GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Payment_StockReservedEventQueue}"));

                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    TotalPrice = context.Message.TotalPrice,
                };

                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = $"Stock miktarı yetersiz..."
                };

                Console.WriteLine("Stock miktarı yetersiz...");

                await publishEndpoint.Publish(stockNotReservedEvent);
            }
        }
    }
}
