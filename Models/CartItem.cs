using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Deen_Essentials.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }  // Tracks which user added this item

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }  // Store price when added
    }
}
