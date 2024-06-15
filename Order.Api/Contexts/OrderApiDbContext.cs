using Microsoft.EntityFrameworkCore;
using Order.Api.Models;

namespace Order.Api.Contexts
{
    public class OrderApiDbContext : DbContext
    {
        public OrderApiDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Models.Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems{ get; set; }
    }
}
