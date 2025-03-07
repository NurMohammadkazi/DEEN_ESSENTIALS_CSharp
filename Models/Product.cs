using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deen_Essentials.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string ProductCategory { get; set; }
        [Required(ErrorMessage = "Product description is required.")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater")]
        public int ProductStock { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}