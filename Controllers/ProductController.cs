using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Security.Claims;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.ViewModels;

namespace TP2.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IProductRepository productRepository, AppDbContext context, ICategoryRepository categoryRepository, IWebHostEnvironment hostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Index
        [AllowAnonymous]
        public IActionResult Index(string searchTerm, int? categoryId, string brand, decimal? minPrice, decimal? maxPrice, int? minYear, int? maxMileage, int sortBy = 0, int page = 1)
        {
            int pageSize = 12; // Increased page size for better UX

            var categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;

            // Get unique brands for filter dropdown
            var brands = _context.CarListings.Select(c => c.Brand).Distinct().Where(b => !string.IsNullOrEmpty(b)).OrderBy(b => b).ToList();
            ViewBag.Brands = brands;

            var productsQuery = _productRepository.GetAllProducts();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchTerm) ||
                                                        p.Description.Contains(searchTerm) ||
                                                        p.Brand.Contains(searchTerm));

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(brand))
                productsQuery = productsQuery.Where(p => p.Brand == brand);

            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

            if (minYear.HasValue)
                productsQuery = productsQuery.Where(p => p.Year >= minYear.Value);

            if (maxMileage.HasValue)
                productsQuery = productsQuery.Where(p => p.Mileage <= maxMileage.Value);

            // Only show approved listings to non-admin users
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                productsQuery = productsQuery.Where(p => p.IsApproved);

            var totalProducts = productsQuery.Count();

            // Apply sorting
            IOrderedQueryable<CarListing> orderedQuery;
            switch (sortBy)
            {
                case 1: // Price: Low to High
                    orderedQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case 2: // Price: High to Low
                    orderedQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case 3: // Oldest
                    orderedQuery = productsQuery.OrderBy(p => p.CreatedAt);
                    break;
                default: // Newest (case 0)
                    orderedQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var products = orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass filter values to view for form repopulation
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.Brand = brand;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinYear = minYear;
            ViewBag.MaxMileage = maxMileage;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;

            return View(products);
        }


        // GET: Details
        public IActionResult Details(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();
            return View(product);

            var categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
        }

        // GET: Create
        [Authorize(Roles = "Admin,Manager,Seller")]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            var categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;
                if (model.ImagePath != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    fileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImagePath.CopyTo(fileStream);
                    }
                }

                var product = new CarListing
                {
                    Name = model.Name,
                    Price = (decimal)model.Price,
                    Mileage = model.Mileage ?? 0,
                    Year = model.Year ?? DateTime.Now.Year,
                    Brand = model.Brand ?? "",
                    FuelType = model.FuelType ?? "",
                    Transmission = model.Transmission ?? "",
                    Color = model.Color ?? "",
                    VIN = model.VIN,
                    EngineSize = model.EngineSize,
                    Horsepower = model.Horsepower,
                    Doors = model.Doors,
                    Seats = model.Seats,
                    Description = model.Description,
                    Features = model.Features,
                    Condition = model.Condition ?? "Used",
                    Rating = model.Rating,
                    IsApproved = User.IsInRole("Admin") || User.IsInRole("Manager"), // Auto-approve for admins/managers
                    SellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    CategoryId = model.CategoryId.Value,
                    Image = fileName,
                    Location = model.Location,
                    QuantityInStock = model.QuantityInStock ?? 1,
                    CreatedAt = DateTime.Now
                };

                _productRepository.Add(product);
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // GET: Edit
        [Authorize(Roles = "Admin,Manager,Seller")]
        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();

            // Check ownership - only owner or admin can edit
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && product.SellerId != userId)
            {
                return Forbid();
            }

            var model = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = (float)product.Price,
                Mileage = product.Mileage,
                Year = product.Year,
                Brand = product.Brand,
                FuelType = product.FuelType,
                Transmission = product.Transmission,
                Color = product.Color,
                VIN = product.VIN,
                EngineSize = product.EngineSize,
                Horsepower = product.Horsepower,
                Doors = product.Doors,
                Seats = product.Seats,
                Description = product.Description,
                Features = product.Features,
                Condition = product.Condition,
                Rating = product.Rating,
                ExistingImagePath = product.Image,
                CategoryId = product.CategoryId,
                Location = product.Location,
                QuantityInStock = product.QuantityInStock
            };

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            var categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = _productRepository.GetById(model.ProductId);
                if (product == null) return NotFound();

                product.Name = model.Name;
                product.Price = (decimal)model.Price;
                product.Mileage = model.Mileage ?? 0;
                product.Year = model.Year ?? DateTime.Now.Year;
                product.Brand = model.Brand ?? "";
                product.FuelType = model.FuelType ?? "";
                product.Transmission = model.Transmission ?? "";
                product.Color = model.Color ?? "";
                product.VIN = model.VIN;
                product.EngineSize = model.EngineSize;
                product.Horsepower = model.Horsepower;
                product.Doors = model.Doors;
                product.Seats = model.Seats;
                product.Description = model.Description;
                product.Features = model.Features;
                product.Condition = model.Condition ?? "Used";
                product.Rating = model.Rating;
                product.Location = model.Location;
                product.UpdatedAt = DateTime.Now;
                product.CategoryId = model.CategoryId.Value;

                if (model.ImagePath != null)
                {
                    // Upload de la nouvelle image
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    string newFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string newFilePath = Path.Combine(uploadsFolder, newFileName);
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        model.ImagePath.CopyTo(fileStream);
                    }

                    // Suppression de l'ancienne image si elle existe
                    if (!string.IsNullOrEmpty(model.ExistingImagePath))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, model.ExistingImagePath);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    product.Image = newFileName;
                }

                _productRepository.Update(product);
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // GET: Delete
        [Authorize(Roles = "Admin,Manager,Seller")]
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();

            // Check ownership - only owner or admin can delete
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && product.SellerId != userId)
            {
                return Forbid();
            }

            var categories = _categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Seller")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();

            // Check ownership - only owner or admin can delete
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && product.SellerId != userId)
            {
                return Forbid();
            }

            _productRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
