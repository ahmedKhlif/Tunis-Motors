using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TP2.Models;

namespace TP2.ViewModels
{
    public class CreateViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "The Car Model field is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Car Model must be between 2 and 100 characters.")]
        [Display(Name = "Car Model")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Price field is required.")]
        [Display(Name = "Price (TND)")]
        [Range(0, double.MaxValue)]
        public float Price { get; set; }

        [Required(ErrorMessage = "The Mileage field is required.")]
        [Display(Name = "Mileage (km)")]
        [Range(0, int.MaxValue)]
        public int? Mileage { get; set; }

        [Required(ErrorMessage = "The Year field is required.")]
        [Display(Name = "Year")]
        [Range(1900, 2030)]
        public int? Year { get; set; }

        [Required(ErrorMessage = "The Brand field is required.")]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "The Fuel Type field is required.")]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string? FuelType { get; set; }

        [Required(ErrorMessage = "The Transmission field is required.")]
        [StringLength(50)]
        [Display(Name = "Transmission")]
        public string? Transmission { get; set; }

        [Required(ErrorMessage = "The Color field is required.")]
        [StringLength(50)]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [StringLength(17)]
        [Display(Name = "VIN Number")]
        public string? VIN { get; set; }

        [Display(Name = "Engine Size (L)")]
        [Range(0, 10)]
        public decimal? EngineSize { get; set; }

        [Display(Name = "Horsepower")]
        [Range(0, 2000)]
        public int? Horsepower { get; set; }

        [Display(Name = "Number of Doors")]
        [Range(2, 5)]
        public int? Doors { get; set; }

        [Display(Name = "Number of Seats")]
        [Range(2, 9)]
        public int? Seats { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Features")]
        [StringLength(500)]
        public string? Features { get; set; }

        [Required(ErrorMessage = "The Condition field is required.")]
        [Display(Name = "Condition")]
        public string? Condition { get; set; } = "Used";

        [Required(ErrorMessage = "The Rating field is required.")]
        [Display(Name = "Rating (Stars)")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "The Image field is required.")]
        [Display(Name = "Main Image")]
        public IFormFile ImagePath { get; set; }

        [Display(Name = "Location")]
        [StringLength(100)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "The Category field is required.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "The Quantity in Stock field is required.")]
        [Display(Name = "Quantity in Stock")]
        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }
    }
}
