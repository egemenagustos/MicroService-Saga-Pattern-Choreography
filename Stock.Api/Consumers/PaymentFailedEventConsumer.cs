using MassTransit;
using Shared.Events;
using Stock.Api.Services;
using MongoDB.Driver;

namespace Stock.Api.Consumers
{
    public class PaymentFailedEventConsumer(MongoDbService mongoDbService) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var stocks = mongoDbService.GetCollection<Models.Stock>();

            foreach(var item in context.Message.OrderItemMessages)
            {
               var stock = await (await stocks.FindAsync(s => s.ProductId == item.ProductId.ToString())).FirstOrDefaultAsync();

                if(stock != null)
                {
                    stock.Count += item.Count;
                    await stocks.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId.ToString(), stock);
                }
            }
        }
    }
}
