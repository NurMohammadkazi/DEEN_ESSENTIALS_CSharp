using System;
using System.ComponentModel.DataAnnotations;

namespace Deen_Essentials.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        // ✅ Fix: Make PhoneNumber and Address optional
        public string? PhoneNumber { get; set; } // Nullable
        public string? Address { get; set; } // Nullable

        // ✅ Ensure CreatedAt is NOT NULL
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
