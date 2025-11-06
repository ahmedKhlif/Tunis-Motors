using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ApprovalController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _context;

        public ApprovalController(IProductRepository productRepository, AppDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        // GET: Pending Approvals
        public IActionResult Index()
        {
            var pendingListings = _productRepository.GetAll()
                .Where(p => !p.IsApproved)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            return View(pendingListings);
        }

        // POST: Approve Listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string adminNote)
        {
            var listing = _productRepository.GetById(id);
            if (listing != null)
            {
                listing.IsApproved = true;
                listing.ApprovedAt = DateTime.Now;
                listing.ApprovedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                listing.AdminApprovalNote = adminNote;
                _productRepository.Update(listing);

                TempData["Success"] = $"Car listing '{listing.Name}' has been approved.";
            }
            return RedirectToAction("Index");
        }

        // POST: Reject Listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string adminNote)
        {
            var listing = _productRepository.GetById(id);
            if (listing != null)
            {
                listing.AdminApprovalNote = adminNote;
                _productRepository.Delete(id);

                TempData["Info"] = $"Car listing '{listing.Name}' has been rejected and removed.";
            }
            return RedirectToAction("Index");
        }

        // GET: Approved Listings
        public IActionResult Approved()
        {
            var approvedListings = _productRepository.GetAll()
                .Where(p => p.IsApproved)
                .OrderByDescending(p => p.ApprovedAt)
                .ToList();
            return View(approvedListings);
        }

        // GET: Listing Details for Approval
        public IActionResult Details(int id)
        {
            var listing = _productRepository.GetById(id);
            if (listing == null) return NotFound();
            return View(listing);
        }
    }
}