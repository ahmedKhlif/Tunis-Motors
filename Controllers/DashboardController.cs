using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP2.Models;
using TP2.ViewModels;

namespace TP2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin Dashboard
        public IActionResult Index()
        {
            var dashboard = new AdminDashboardViewModel
            {
                TotalListings = _context.CarListings.Count(),
                PendingApprovals = _context.CarListings.Count(c => !c.IsApproved),
                ApprovedListings = _context.CarListings.Count(c => c.IsApproved),
                TotalUsers = _context.Users.Count(),
                TotalOrders = _context.Orders.Count(),
                TotalMessages = _context.Messages.Count(),
                RecentListings = _context.CarListings
                    .Include(c => c.Seller)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToList(),
                RecentOrders = _context.Orders
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToList(),
                PendingMessages = _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => !m.IsRead)
                    .OrderByDescending(m => m.SentAt)
                    .Take(5)
                    .ToList()
            };

            return View(dashboard);
        }

        // GET: Analytics
        public IActionResult Analytics()
        {
            var analytics = new AnalyticsViewModel
            {
                MonthlyRevenue = _context.Orders
                    .Where(o => o.OrderDate.Month == DateTime.Now.Month && o.OrderDate.Year == DateTime.Now.Year)
                    .Sum(o => (decimal)o.TotalAmount),
                TotalRevenue = _context.Orders.Sum(o => (decimal)o.TotalAmount),
                PopularBrands = _context.CarListings
                    .Where(c => c.IsApproved)
                    .GroupBy(c => c.Brand)
                    .Select(g => new BrandStats { Brand = g.Key ?? "Unknown", Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToList(),
                ListingsByCategory = _context.CarListings
                    .Include(c => c.Category)
                    .Where(c => c.IsApproved)
                    .GroupBy(c => c.Category.CategoryName)
                    .Select(g => new CategoryStats { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .ToList(),
                UserRegistrations = _context.Users
                    .GroupBy(u => u.Id) // Temporary fix - need to add registration date to user profile
                    .Select(g => new MonthlyStats { Month = DateTime.Now.Month, Count = g.Count() })
                    .Take(1)
                    .ToList()
            };

            return View(analytics);
        }

        // GET: User Management
        public IActionResult Users()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                userList.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = string.Join(", ", roles),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled
                });
            }

            return View(userList);
        }

        // GET: System Settings
        public IActionResult Settings()
        {
            return View();
        }
    }
}

namespace TP2.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalListings { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedListings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalMessages { get; set; }
        public List<CarListing> RecentListings { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Message> PendingMessages { get; set; }
    }

    public class AnalyticsViewModel
    {
        public decimal MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<BrandStats> PopularBrands { get; set; }
        public List<CategoryStats> ListingsByCategory { get; set; }
        public List<MonthlyStats> UserRegistrations { get; set; }
    }

    public class BrandStats
    {
        public string Brand { get; set; }
        public int Count { get; set; }
    }

    public class CategoryStats
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyStats
    {
        public int Month { get; set; }
        public int Count { get; set; }
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
    }
}