namespace RRS.Models.ViewModels
{
    public class PaymentDetailsViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string ModeOfPayment { get; set; }
        public string ReservationNumber { get; set; }
        public string UserFullName { get; set; }
        public string CustomerFullName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
