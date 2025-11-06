using Microsoft.EntityFrameworkCore;
using TP2.Models;

using CarListing = TP2.Models.CarListing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TP2.Models.Repositories
{
    public class ProductRepository : IProductRepository
    {
        readonly AppDbContext context;

        public ProductRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IList<CarListing> GetAll()
        {
            return context.CarListings
                .Include(x => x.Category)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public CarListing GetById(int id)
        {
            return context.CarListings
                .Include(x => x.Category)
                .SingleOrDefault(x => x.ProductId == id);
        }

        public void Add(CarListing p)
        {
            context.CarListings.Add(p);
            context.SaveChanges();
        }

        public IList<CarListing> FindByName(string name)
        {
            return context.CarListings
                .Include(c => c.Category)
                .Where(p => p.Name.Contains(name) || p.Category.CategoryName.Contains(name))
                .ToList();
        }

        public CarListing Update(CarListing p)
        {
            CarListing p1 = context.CarListings.Find(p.ProductId);
            if (p1 != null)
            {
                p1.Name = p.Name;
                p1.Price = p.Price;
                p1.Mileage = p.Mileage;
                p1.Year = p.Year;
                p1.Brand = p.Brand;
                p1.FuelType = p.FuelType;
                p1.Transmission = p.Transmission;
                p1.Color = p.Color;
                p1.VIN = p.VIN;
                p1.EngineSize = p.EngineSize;
                p1.Horsepower = p.Horsepower;
                p1.Doors = p.Doors;
                p1.Seats = p.Seats;
                p1.Description = p.Description;
                p1.Features = p.Features;
                p1.Condition = p.Condition;
                p1.Location = p.Location;
                p1.UpdatedAt = DateTime.Now;
                p1.CategoryId = p.CategoryId;
                context.SaveChanges();
            }
            return p1;
        }

        public void Delete(int ProductId)
        {
            CarListing p1 = context.CarListings.Find(ProductId);
            if (p1 != null)
            {
                context.CarListings.Remove(p1);
                context.SaveChanges();
            }
        }

        public IList<CarListing> GetProductsByCategID(int? CategId)
        {
            return context.CarListings
                .Include(p => p.Category)
                .Where(p => p.CategoryId == CategId)
                .OrderBy(p => p.ProductId)
                .ToList();
        }

        public IQueryable<CarListing> GetAllProducts()
        {
            return context.CarListings.Include(p => p.Category);
        }
    }
}
