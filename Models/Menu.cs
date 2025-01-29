using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RRS.Models
{
    public class Menu
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        [Required, StringLength(100)]
        public string Category { get; set; }
        [Required, Precision(10, 2)]
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
