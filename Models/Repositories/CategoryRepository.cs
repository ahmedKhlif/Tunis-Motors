using Microsoft.EntityFrameworkCore;

namespace TP2.Models.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        readonly AppDbContext context;
        public CategoryRepository(AppDbContext context)
        {
            this.context = context;
        }
        public IList<Category> GetAll()
        {
            return context.Categories
            .Include(c => c.CarListings)
            .OrderBy(c => c.CategoryName).ToList();
        }
        public Category GetById(int id)
        {
            return context.Categories.Find(id);
        }
        public void Add(Category category)
        {
            context.Categories.Add(category);
            context.SaveChanges();
        }

        public Category Update(Category c)
        {
            var existingCategory = context.Categories.Find(c.CategoryId);
            if (existingCategory != null)
            {
                existingCategory.CategoryName = c.CategoryName;
                existingCategory.Image = c.Image;
                context.SaveChanges();
            }
            return existingCategory;
        }

        public void Delete(int CategoryId)
        {
            Category c1 = context.Categories.Find(CategoryId);
            if (c1 != null)
            {
                context.Categories.Remove(c1);
                context.SaveChanges();
            }
        }
    }
}
