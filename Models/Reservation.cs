using Microsoft.EntityFrameworkCore;

namespace RRS.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public DateOnly ReservationDate { get; set; }
        public TimeOnly ReservationTime { get; set; }
        //public string? OccasionType { get; set; }  
        [Precision(10, 2)]
        public Decimal TotalPrice { get; set; }
        public string? BuffetType { get; set; }
        public string? SpecialRequest { get; set; }
        public string Status { get; set; }

        // Foreign key for table
        public int TableId { get; set; }
        public Table Table { get; set; }


        // Foreign key for customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

}