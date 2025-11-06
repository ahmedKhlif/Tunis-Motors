using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TP2.Models;
using TP2.ViewModels;

namespace TP2.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MessageController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Inbox
        public IActionResult Inbox()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Listing)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return View(messages);
        }

        // GET: Sent Messages
        public IActionResult Sent()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Listing)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return View(messages);
        }

        // GET: Compose Message
        public async Task<IActionResult> Compose(int? listingId)
        {
            var model = new TP2.ViewModels.MessageViewModel();

            // Get all users with their roles for the dropdown
            var usersWithRoles = await (from user in _context.Users
                                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                       join role in _context.Roles on userRole.RoleId equals role.Id
                                       where role.Name != "Buyer" // Exclude buyers from recipient list
                                       select new
                                       {
                                           Id = user.Id,
                                           UserName = user.UserName,
                                           Email = user.Email,
                                           Role = role.Name
                                       }).ToListAsync();

            ViewBag.Receivers = usersWithRoles;

            if (listingId.HasValue)
            {
                var listing = _context.CarListings
                    .Include(l => l.Seller)
                    .FirstOrDefault(l => l.ProductId == listingId);

                if (listing != null)
                {
                    model.ReceiverId = listing.SellerId;
                    model.ListingId = listingId;
                    model.Subject = $"Inquiry about {listing.Name}";
                }
            }
            return View(model);
        }

        // POST: Compose Message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(TP2.ViewModels.MessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new Message
                {
                    SenderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ReceiverId = model.ReceiverId,
                    Subject = model.Subject,
                    Content = model.Content,
                    ListingId = model.ListingId,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Message sent successfully";
                return RedirectToAction("Inbox");
            }
            return View(model);
        }

        // GET: Message Details
        public IActionResult Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var message = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Listing)
                .FirstOrDefault(m => m.Id == id && (m.SenderId == userId || m.ReceiverId == userId));

            if (message == null) return NotFound();

            // Mark as read if recipient is viewing
            if (message.ReceiverId == userId && !message.IsRead)
            {
                message.IsRead = true;
                _context.SaveChanges();
            }

            return View(message);
        }

        // POST: Delete Message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var message = _context.Messages
                .FirstOrDefault(m => m.Id == id && (m.SenderId == userId || m.ReceiverId == userId));

            if (message != null)
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
                TempData["Success"] = "Message deleted successfully";
            }

            return RedirectToAction("Inbox");
        }

        // GET: Unread Count (for AJAX)
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var count = _context.Messages.Count(m => m.ReceiverId == userId && !m.IsRead);
            return Json(new { count });
        }
    }
}