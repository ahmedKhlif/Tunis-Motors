using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.Services;

namespace TP2.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public OrderController(IOrderRepository orderRepository, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Order/Index - Admin/Manager Order Management
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Index(string status = null, string search = null, int page = 1)
        {
            var orders = _orderRepository.GetAll();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                orders = orders.Where(o => o.Status == orderStatus);
            }

            // Filter by search term (customer name or email)
            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    o.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Sort by order date (newest first)
            orders = orders.OrderByDescending(o => o.OrderDate);

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;

            return View(orders.ToList());
        }

        // GET: Order/Details/5 - View Order Details
        public IActionResult Details(int id)
        {
            var order = _orderRepository.GetById(id);
            if (order == null)
                return NotFound();

            // Check if user can view this order (own order or admin/manager)
            var user = _userManager.GetUserAsync(User).Result;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");

            if (!isAdmin && !isManager && order.UserId != user.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        // POST: Order/UpdateStatus/5 - Update Order Status
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus, string notes = null)
        {
            var order = _orderRepository.GetById(id);
            if (order == null)
                return NotFound();

            var oldStatus = order.Status; // Store old status for comparison

            try
            {
                var user = await _userManager.GetUserAsync(User);
                order.Status = newStatus;
                order.StatusUpdatedAt = DateTime.Now;
                order.StatusUpdatedBy = user.UserName;

                if (!string.IsNullOrEmpty(notes))
                {
                    order.Notes = notes;
                }

                _orderRepository.Update(order);

                // Send email notification to buyer if status changed
                if (oldStatus != newStatus)
                {
                    try
                    {
                        await _emailService.SendOrderStatusUpdateAsync(
                            order.Email,
                            order.CustomerName,
                            order.Id,
                            newStatus.ToString()
                        );
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the status update
                        Console.WriteLine($"Email notification failed: {emailEx.Message}");
                    }
                }

                TempData["Success"] = $"Order status updated to {newStatus}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating order status: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Order/MyOrders - User's Order History
        public IActionResult MyOrders(int page = 1)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var orders = _orderRepository.GetAll()
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.CurrentPage = page;
            return View(orders);
        }

        // POST: Order/Cancel/5 - Cancel Order (by user)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id, string reason = null)
        {
            var order = _orderRepository.GetById(id);
            if (order == null)
                return NotFound();

            var user = _userManager.GetUserAsync(User).Result;
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");

            // Check permissions
            if (!isAdmin && !isManager && order.UserId != user.Id)
            {
                return Forbid();
            }

            // Check if order can be cancelled
            if (!order.CanBeCancelled)
            {
                TempData["Error"] = "This order cannot be cancelled at this stage.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                order.Status = OrderStatus.Cancelled;
                order.StatusUpdatedAt = DateTime.Now;
                order.StatusUpdatedBy = isAdmin || isManager ? user.UserName : "Customer";

                if (!string.IsNullOrEmpty(reason))
                {
                    order.Notes = $"Cancelled by {(isAdmin || isManager ? "Admin" : "Customer")}: {reason}";
                }

                _orderRepository.Update(order);

                TempData["Success"] = "Order cancelled successfully";
                return RedirectToAction(isAdmin || isManager ? nameof(Index) : nameof(MyOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling order: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Order/OrderDetails/5 - Alias for Details
        public IActionResult OrderDetails(int id)
        {
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}