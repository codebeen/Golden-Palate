using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RRS.Data;
using RRS.Models;
using RRS.Models.ViewModels;
using System.Data;
using System.Security.Claims;

namespace RRS.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffReservationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<StaffReservationController> logger;

        public StaffReservationController(ApplicationDbContext context, ILogger<StaffReservationController> logger)
        {
            this.context = context;
            this.logger = logger;
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
            var getSpecificReservation = context.ReservationDetails.FromSqlRaw("GetReservationDetailsById @p0", id).AsEnumerable().FirstOrDefault();

            if (getSpecificReservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found";

                return RedirectToAction("Index");
            }

            return PartialView("ReservationDetails", getSpecificReservation);
        }

        public IActionResult ShowPaymentMethodModal(int id)
        {
            var reservationToPay = context.ReservationDetails.FromSqlRaw("GetReservationDetailsById @p0", id).AsEnumerable().FirstOrDefault();

            if (reservationToPay == null)
            {
                TempData["ErrorMessage"] = "Reservation not found";

                return RedirectToAction("Index");
            }

            return View("PaymentMethodModal", reservationToPay);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShowCashModal(int id)
        {

            var reservationToPay = context.ReservationDetails.FromSqlRaw("GetReservationDetailsById @p0", id).AsEnumerable().FirstOrDefault();

            if (reservationToPay == null)
            {
                TempData["ErrorMessage"] = "Reservation not found";

                return RedirectToAction("Index");
            }

            //Console.WriteLine(reservationToPay.TotalPrice);
            //Console.WriteLine(reservationToPay.ReservationNumber);
            //Console.WriteLine(reservationToPay.Id);
            //Console.WriteLine(reservationToPay.ReservationNumber);

            //ViewData["cashAmount"] = reservationToPay.TotalPrice;
            //ViewData["cashDescription"] = $"Payment for {reservationToPay.ReservationNumber}";
            //ViewData["cashReservationId"] = reservationToPay.Id;
            //ViewData["cashReservationNumber"] = reservationToPay.ReservationNumber;

            return PartialView("CashModal", reservationToPay);
        }

        [HttpPost]
        public IActionResult StartReservation(int reservationId)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

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
                var errorlogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to start reservation" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

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
                var errorlogParams = new SqlParameter[]
                {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to start reservation" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                TempData["ErrorMessage"] = "Too early to start the reservation.";
                return RedirectToAction("Index");
            }
            else if (currentTime > endTime)
            {
                var errorlogParams = new SqlParameter[]
                {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to start reservation" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                TempData["ErrorMessage"] = "Too late to start the reservation.";
                return RedirectToAction("Index");
            }

            // If valid, update the reservation status
            this.UpdateReservationStatus(reservationId, status);
            TempData["SuccessMessage"] = "Successfully started the reservation.";

            var logParams = new SqlParameter[]
            {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Start a reservation" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

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


        [HttpGet]
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
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var cancelReservationResult = context.Database.ExecuteSqlRaw($"Exec CancelReservationById @p0", reservationId);

            if (cancelReservationResult == 1)
            {

                var logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Cancelled a reservation" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                TempData["SuccessMessage"] = "Reservation successfully cancelled.";
                return RedirectToAction("Index");
            }

            var errorlogParams = new SqlParameter[]
            {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to cancel a reservation" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

            TempData["ErrorMessage"] = "Unable to cancel reservation.";
            return RedirectToAction("Index");
        }

    }
}
