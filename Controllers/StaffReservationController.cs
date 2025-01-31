using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models.ViewModels;

namespace RRS.Controllers
{
    public class StaffReservationController : Controller
    {
        private readonly ApplicationDbContext context;

        public StaffReservationController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            // Fetch ReservationDetails view model from the stored procedure or database.
            var reservationDetails = context.ReservationDetails.FromSqlRaw("GetReservationDetails").ToList();

            // Pass the correct model to the view.
            return View("Index", reservationDetails);  // Ensure it's IEnumerable<ReservationDetails>
        }

        public IActionResult Dashboard()
        {
            DashboardViewModel dashboardViewModel = new DashboardViewModel();

            dashboardViewModel.counOfTodaysReservation = context.ReservationDetails.FromSqlRaw("GetTodaysReservations").AsEnumerable().Count();
            dashboardViewModel.counOfCompletedReservation = context.ReservationDetails.FromSqlRaw("GetCompletedReservations").AsEnumerable().Count();
            dashboardViewModel.counOfCancelledReservation = context.ReservationDetails.FromSqlRaw("GetCancelledReservations").AsEnumerable().Count();
            dashboardViewModel.counOfOngoingReservation = context.ReservationDetails.FromSqlRaw("GetOngoingReservations").AsEnumerable().Count();
            dashboardViewModel.counOfUpcomingReservation = context.ReservationDetails.FromSqlRaw("GetUpcomingReservations").AsEnumerable().Count();
            dashboardViewModel.reservationDetails = context.ReservationDetails.FromSqlRaw("GetUpcomingReservations").ToList();

            return View("Dashboard", dashboardViewModel);
        }

        public IActionResult ViewReservationDetailsStaff(int id)
        {
            var getSpecificReservation = context.ReservationDetails.FromSqlRaw($"GetReservationDetailsById {id}").AsEnumerable().FirstOrDefault();

            if (getSpecificReservation != null)
            {
                TempData["ErrorMessage"] = "Reservation not found";

                return RedirectToAction("Index");
            }

            return PartialView("ReservationDetails", getSpecificReservation);
        }

        [HttpPost]
        public IActionResult StartReservation(int reservationId)
        {
            Console.WriteLine(reservationId);
            var status = "Ongoing";

            var parameter = new SqlParameter[]
            {
                new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = reservationId
                },
                new SqlParameter()
                {
                    ParameterName = "@Status",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Value = status
                },
            };

            var updateReservationResult = context.Database.ExecuteSqlRaw("UpdateReservationStatus @Id, @Status", parameter);

            if (updateReservationResult == 1)
            {
                TempData["SuccessMessage"] = "Successfully started the reservation.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Unable to start the reservation.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CompleteReservation(int reservationId)
        {
            var status = "Completed";

            var parameter = new SqlParameter[]
            {
                new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = reservationId
                },
                new SqlParameter()
                {
                    ParameterName = "@Status",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Value = status
                },
            };

            var updateReservationResult = context.Database.ExecuteSqlRaw("UpdateReservationStatus @Id, @Status", parameter);

            if (updateReservationResult == 1)
            {
                TempData["SuccessMessage"] = "Successfully completed the reservation.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Unable to complete the reservation.";
            return RedirectToAction("Index");
        }

        public IActionResult GetUpcomingReservations()
        {
            var upcomingReservations = context.ReservationDetails.FromSqlRaw("GetUpcomingReservations").ToList();

            return View("UpcomingReservations", upcomingReservations);
        }


        public IActionResult GetTodaysReservations()
        {
            var todaysReservations = context.ReservationDetails.FromSqlRaw("GetTodaysReservations").ToList();

            return View("TodaysReservations", todaysReservations);
        }


        public IActionResult GetCompletedReservations()
        {
            var completedReservations = context.ReservationDetails.FromSqlRaw("GetCompletedReservations").ToList();

            return View("CompletedReservations", completedReservations);
        }

        public IActionResult GetOngoingReservations()
        {
            var ongoingReservations = context.ReservationDetails.FromSqlRaw("GetOngoingReservations").ToList();

            return View("OngoingReservations", ongoingReservations);
        }

        public IActionResult GetCancelledReservations()
        {
            var ongoingReservations = context.ReservationDetails.FromSqlRaw("GetCancelledReservations").ToList();

            return View("CancelledReservations", ongoingReservations);
        }
    }
}
