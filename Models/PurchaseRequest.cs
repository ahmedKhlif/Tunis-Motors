using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP2.Models
{
    public class PurchaseRequest
    {
        public int PurchaseRequestId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser Customer { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual CarListing CarListing { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, Responded, Closed

        [Display(Name = "Seller Response")]
        [StringLength(1000)]
        public string? SellerResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RespondedAt { get; set; }
    }
}