using System.ComponentModel.DataAnnotations;

namespace TP2.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
        public string CategoryName { get; set; }

        [Display(Name = "Category Image")]
        [StringLength(500)]
        public string? Image { get; set; }

        public ICollection<CarListing> CarListings { get; set; }
    }
}
