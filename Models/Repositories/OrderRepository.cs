using Microsoft.EntityFrameworkCore;

namespace TP2.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        readonly AppDbContext context;
        public OrderRepository(AppDbContext context)
        {
            this.context = context;
        }

        public void Add(Order o)
        {
            context.Orders.Add(o);
            context.SaveChanges();
        }

        public Order GetById(int id)
        {
            return context.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);
        }

        public void Update(Order o)
        {
            context.Orders.Update(o);
            context.SaveChanges();
        }

        public IEnumerable<Order> GetAll()
        {
            return context.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .AsEnumerable();
        }

        public IEnumerable<Order> GetByUserId(string userId)
        {
            return context.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .AsEnumerable();
        }

        public void Delete(int id)
        {
            var order = context.Orders.Find(id);
            if (order != null)
            {
                context.Orders.Remove(order);
                context.SaveChanges();
            }
        }
    }
}
