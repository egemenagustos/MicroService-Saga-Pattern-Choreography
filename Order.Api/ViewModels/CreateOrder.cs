namespace Order.Api.ViewModels
{
    public class CreateOrder
    {
        public string BuyerId { get; set; }

        public List<CreateOrderItem> CreateOrderItems { get; set; }
    }
}
