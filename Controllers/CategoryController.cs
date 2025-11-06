using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {

        readonly ICategoryRepository CategRepository;
        public CategoryController(ICategoryRepository categRepository)
        {
            CategRepository = categRepository;
        }
        // GET: CategoryController
        [AllowAnonymous]
        public ActionResult Index()
        {
            var Categories = CategRepository.GetAll();
            return View(Categories);
        }

        // GET: CategoryController/Details/5
        public ActionResult Details(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null) return NotFound(); 
            return View(category);

            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            return View();

            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Category category, IFormFile? ImageFile)
        {
            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        return View(category);
                    }

                    // Validate file size (max 5MB)
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                        return View(category);
                    }

                    // Generate unique filename
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                    // Ensure directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    category.Image = "/images/categories/" + fileName;
                }

                // Ajouter la catégorie dans la base via le repository
                CategRepository.Add(category);

                // Set success message
                TempData["Success"] = "Category created successfully!";

                // Rediriger vers la liste
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Error creating category. Please try again.";
                return View(category);
            }
        }


        // GET: CategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);

            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Category category, IFormFile? ImageFile)
        {
            try
            {
                // Get the existing category from database to preserve current image if no new one uploaded
                var existingCategory = CategRepository.GetById(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Preserve existing image if no new file uploaded
                if (ImageFile == null || ImageFile.Length == 0)
                {
                    category.Image = existingCategory.Image;
                }
                else
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        return View(category);
                    }

                    // Validate file size (max 5MB)
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                        return View(category);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingCategory.Image))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCategory.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Generate unique filename
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                    // Ensure directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    category.Image = "/images/categories/" + fileName;
                }

                // Ensure the ID is set correctly
                category.CategoryId = id;

                CategRepository.Update(category);
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Error updating category. Please try again.";
                return View(category);
            }
        }


        // GET: CategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);

            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
        }

        // POST: CategoryController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            CategRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
