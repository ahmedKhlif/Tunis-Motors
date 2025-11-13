namespace TP2.Models.Repositories
{
    public interface IOrderRepository
    {
        Order GetById(int Id);
        void Add(Order o);
        void Update(Order o);
        IEnumerable<Order> GetAll();
        IEnumerable<Order> GetByUserId(string userId);
        void Delete(int id);
    }
}
