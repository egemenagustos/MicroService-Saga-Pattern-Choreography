namespace Order.Api.ViewModels
{
    public class CreateOrderItem
    {
        public string ProductId { get; set; }

        public int Count { get; set; }

        public decimal Price { get; set; }
    }
}
