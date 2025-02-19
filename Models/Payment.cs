using Microsoft.EntityFrameworkCore;

namespace RRS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [Precision(10, 2)]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int ReservationId { get; set; }
        public int UserId { get; set; } 
        public string ModeOfPayment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        
        public Reservation Reservation { get; set; }
        public User User { get; set; }

    }
}
