using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;

        public WishlistController(AppDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        // GET: My Wishlist
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var wishlistItems = _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.CarListing)
                .ThenInclude(c => c.Category)
                .ToList();

            return View(wishlistItems);
        }

        // POST: Add to Wishlist
        [HttpPost]
        public IActionResult AddToWishlist(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingItem = _context.Wishlists
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (existingItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.Now
                };
                _context.Wishlists.Add(wishlistItem);
                _context.SaveChanges();
            }

            return Json(new { success = true, message = "Added to wishlist" });
        }

        // POST: Remove from Wishlist
        [HttpPost]
        public IActionResult RemoveFromWishlist(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var wishlistItem = _context.Wishlists
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                _context.SaveChanges();
            }

            return Json(new { success = true, message = "Removed from wishlist" });
        }

        // Check if product is in wishlist
        [HttpGet]
        public IActionResult IsInWishlist(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isInWishlist = _context.Wishlists
                .Any(w => w.UserId == userId && w.ProductId == productId);

            return Json(new { isInWishlist });
        }
    }
}