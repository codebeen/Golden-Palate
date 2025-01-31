using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models.ViewModels;

namespace RRS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext context;

        public DashboardController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            AdminDashboardViewModel dashboardViewModel = new AdminDashboardViewModel();

            dashboardViewModel.availableTables = context.Tables.FromSqlRaw("GetAvailableTables").AsEnumerable().Count();
            dashboardViewModel.totalTables = context.Tables.FromSqlRaw("GetAllTables").AsEnumerable().Count();
            dashboardViewModel.totalReservations = context.Reservations.FromSqlRaw("GetAllNotCancelledReservations").AsEnumerable().Count();
            dashboardViewModel.counOfTodaysReservation = context.ReservationDetails.FromSqlRaw("GetTodaysReservations").AsEnumerable().Count();
            dashboardViewModel.counOfCompletedReservation = context.ReservationDetails.FromSqlRaw("GetCompletedReservations").AsEnumerable().Count();
            dashboardViewModel.counOfCancelledReservation = context.ReservationDetails.FromSqlRaw("GetCancelledReservations").AsEnumerable().Count();
            dashboardViewModel.counOfOngoingReservation = context.ReservationDetails.FromSqlRaw("GetOngoingReservations").AsEnumerable().Count();
            dashboardViewModel.counOfUpcomingReservation = context.ReservationDetails.FromSqlRaw("GetUpcomingReservations").AsEnumerable().Count();
            dashboardViewModel.reservationDetails = context.ReservationDetails.FromSqlRaw("GetUpcomingReservations").ToList();

            return View("Index", dashboardViewModel);
        }
    }
}
