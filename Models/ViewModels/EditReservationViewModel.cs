namespace RRS.Models.ViewModels
{
    public class EditReservationViewModel
    {
        public List<DateOnly> ReservedDates { get; set; } = new List<DateOnly>();
        public ReservationDetailsDto ReservationDetails { get; set; }
    }
}
