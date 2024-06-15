using MassTransit;
using Order.Api.Contexts;
using Shared.Events;

namespace Order.Api.Consumers
{
    public class PaymentComletedEventConsumer(OrderApiDbContext orderApiDbContext) : IConsumer<PaymentCompletedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await orderApiDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order is null)
                throw new Exception("Order is not found");

            order.OrderStatus = Enums.OrderStatus.Completed;
            await orderApiDbContext.SaveChangesAsync();
        }
    }
}
