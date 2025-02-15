namespace RRS.Models.ViewModels
{
    public class ReservationDetailsDto
    {
        public int Id { get; set; }
        public string ReservationNumber { get; set; }
        public DateOnly ReservationDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string BuffetType { get; set; }
        public string? SpecialRequest { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Customer details
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Table details
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public int SeatingCapacity { get; set; }
        public string? Description { get; set; }
        public string TableLocation { get; set; }
        public string TableStatus { get; set; }
    }

}
