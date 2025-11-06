using TP2.Models;

using CarListing = TP2.Models.CarListing;

namespace TP2.ViewModels
{
    public class ProduitPaginationViewModel
    {
        public List<CarListing> Products { get; set; }
        public int PageActuelle { get; set; }
        public int TotalPages { get; set; }
    }
}