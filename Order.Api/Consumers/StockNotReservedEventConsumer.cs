using MassTransit;
using Order.Api.Contexts;
using Shared.Events;

namespace Order.Api.Consumers
{
    public class StockNotReservedEventConsumer(OrderApiDbContext orderApiDbContext) : IConsumer<StockNotReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var order = await orderApiDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order is null)
                throw new Exception("Order is not found");

            order.OrderStatus = Enums.OrderStatus.Fail;
            await orderApiDbContext.SaveChangesAsync();
        }
    }
}
