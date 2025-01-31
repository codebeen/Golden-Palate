namespace RRS.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int availableTables { get; set; }
        public int totalReservations { get; set; }
        public int totalTables { get; set; }
        public int counOfTodaysReservation { get; set; }
        public int counOfUpcomingReservation { get; set; }
        public int counOfCompletedReservation { get; set; }
        public int counOfCancelledReservation { get; set; }
        public int counOfOngoingReservation { get; set; }

        public List<ReservationDetailsViewModel> reservationDetails { get; set; }
    }
}
