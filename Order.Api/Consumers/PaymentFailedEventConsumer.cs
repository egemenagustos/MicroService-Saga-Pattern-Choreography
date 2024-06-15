using MassTransit;
using Order.Api.Contexts;
using Shared.Events;

namespace Order.Api.Consumers
{
    public class PaymentFailedEventConsumer(OrderApiDbContext orderApiDbContext) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await orderApiDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order is null)
                throw new Exception("Order is not found");

            order.OrderStatus = Enums.OrderStatus.Fail;
            await orderApiDbContext.SaveChangesAsync();
        }
    }
}
