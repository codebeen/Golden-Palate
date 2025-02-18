using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
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
            var reservationDetails = context.ReservationDetails.FromSqlRaw("GetReservationDetailsFromView").ToList();

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

            // Check if the reservation exists
            var reservation = context.ReservationDetails.FromSqlRaw($"Exec GetReservationDetailsById {reservationId}").AsEnumerable().FirstOrDefault();

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Unable to find reservation.";
                return RedirectToAction("Index");
            }

            // Ensure reservation date is parsed correctly
            DateTime reservationDate;
            if (reservation.ReservationDate is DateOnly)
            {
                reservationDate = reservation.ReservationDate.ToDateTime(new TimeOnly(0, 0));  // Midnight time
            }
            else
            {
                reservationDate = DateTime.Today;  // Default to today if invalid
            }

            // Ensure reservation is for today
            if (reservationDate.Date != DateTime.Today)
            {
                TempData["ErrorMessage"] = "You cannot start the reservation because the reservation date is not today.";
                return RedirectToAction("Index");
            }

            // Get current time
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            // Define buffet time slots
            TimeSpan startTime, endTime;

            switch (reservation.BuffetType)
            {
                case "Breakfast":
                    startTime = new TimeSpan(8, 0, 0);   // 8:00 AM
                    endTime = new TimeSpan(8, 30, 0);    // 8:30 AM
                    break;
                case "Lunch":
                    startTime = new TimeSpan(11, 30, 0); // 11:30 AM
                    endTime = new TimeSpan(12, 0, 0);    // 12:00 PM
                    break;
                case "Dinner":
                    startTime = new TimeSpan(17, 0, 0);  // 5:00 PM
                    endTime = new TimeSpan(17, 30, 0);   // 5:30 PM
                    break;
                default:
                    TempData["ErrorMessage"] = "Invalid buffet type.";
                    return RedirectToAction("Index");
            }

            // Check if the current time is within the allowed range
            if (currentTime < startTime)
            {
                TempData["ErrorMessage"] = "Too early to start the reservation.";
                return RedirectToAction("Index");
            }
            else if (currentTime > endTime)
            {
                TempData["ErrorMessage"] = "Too late to start the reservation.";
                return RedirectToAction("Index");
            }

            // If valid, update the reservation status
            this.UpdateReservationStatus(reservationId, status);
            TempData["SuccessMessage"] = "Successfully started the reservation.";
            return RedirectToAction("Index");
        }


        private IActionResult UpdateReservationStatus(int reservationId, string status)
        {
            // SQL parameters for updating reservation status
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

            // Execute the update query
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

        public IActionResult CancelReservation(int reservationId)
        {
            var cancelReservationResult = context.Database.ExecuteSqlRaw($"Exec CancelReservationById @p0", reservationId);

            if (cancelReservationResult == 1)
            {
                TempData["SuccessMessage"] = "Reservation successfully cancelled.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Unable to cancel reservation.";
            return RedirectToAction("Index");
        }

    }
}
