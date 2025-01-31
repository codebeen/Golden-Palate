namespace RRS.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int counOfTodaysReservation {  get; set; }
        public int counOfUpcomingReservation { get; set; }
        public int counOfCompletedReservation { get; set; }
        public int counOfCancelledReservation { get; set; }
        public int counOfOngoingReservation { get; set; }

        public List<ReservationDetailsViewModel> reservationDetails { get; set; }
    }
}
