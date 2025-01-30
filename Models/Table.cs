using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRS.Models
{
    public class Table
    {
        public int Id { get; set; }
        [Required]
        public int TableNumber { get; set; }
        public string? Description { get; set; }
        [Required]
        public int SeatingCapacity { get; set; }
        [Required, StringLength(100)]
        public string TableLocation { get; set; }
        [Precision(10, 2)]
        public Decimal Price { get; set; }
        public string? TableImagePath { get; set; }
        public string? Status { get; set; } 
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [NotMapped] // This property won't be mapped to the database
        public IFormFile? TableImageFile { get; set; }
    }

}