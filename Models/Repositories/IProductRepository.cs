using System.Collections.Generic;
using TP2.Models;

using CarListing = TP2.Models.CarListing;

namespace TP2.Models.Repositories
{
    public interface IProductRepository
    {
        CarListing GetById(int Id);
        IList<CarListing> GetAll();
        void Add(CarListing t);
        CarListing Update(CarListing t);
        void Delete(int Id);
        IList<CarListing> GetProductsByCategID(int? CategId);
        IList<CarListing> FindByName(string name);
        public IQueryable<CarListing> GetAllProducts();
    }
}
