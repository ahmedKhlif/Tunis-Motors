using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;

        public ProfileController(UserManager<IdentityUser> userManager, AppDbContext context, IProductRepository productRepository)
        {
            _userManager = userManager;
            _context = context;
            _productRepository = productRepository;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Id == user.Id);

            if (profile == null)
            {
                profile = new UserProfile { Id = user.Id, CreatedAt = DateTime.Now };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        // POST: Profile Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfile model)
        {
            if (ModelState.IsValid)
            {
                var profile = await _context.UserProfiles.FindAsync(model.Id);
                if (profile != null)
                {
                    profile.FirstName = model.FirstName;
                    profile.LastName = model.LastName;
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.Address = model.Address;
                    profile.City = model.City;
                    profile.Country = model.Country;
                    profile.DateOfBirth = model.DateOfBirth;
                    profile.Bio = model.Bio;
                    profile.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Profile updated successfully";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        // GET: My Listings (for sellers)
        [Authorize(Roles = "Seller,Admin,Manager")]
        public IActionResult MyListings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var listings = _productRepository.GetAll()
                .Where(c => c.SellerId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(listings);
        }

        // GET: My Orders (for buyers)
        public IActionResult MyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orders = _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Order Details
        public IActionResult OrderDetails(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: My Messages
        public IActionResult MyMessages()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Listing)
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return View(messages);
        }
    }
}