using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models.ViewModels;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace RRS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReservationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AdminReservationController> logger;

        public AdminReservationController(ApplicationDbContext context, ILogger<AdminReservationController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            // Fetch ReservationDetails view model from the stored procedure or database.
            var reservationDetails = context.ReservationDetails.FromSqlRaw("EXEC GetReservationDetailsFromView").ToList();

            // Pass the correct model to the view.
            return View("Index", reservationDetails);  // Ensure it's IEnumerable<ReservationDetails>
        }

        public IActionResult ViewReservationDetails(int id)
        {
            var getSpecificReservation = context.ReservationDetails.FromSqlRaw("GetReservationDetailsById @p0", id).AsEnumerable().FirstOrDefault();

            if (getSpecificReservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found";
                return RedirectToAction("Index");
            }

            return PartialView("ReservationDetails", getSpecificReservation);
        }


        public ActionResult Export()
        {
            var reservations = context.ReservationDetails.FromSqlRaw("EXEC GetReservationDetailsFromView").ToList();

            var csvFileName = $"reservations_{DateTime.Now:yyyy-MM-dd}.csv";
            var csvContent = new StringBuilder();

            // Add CSV headers
            csvContent.AppendLine("Reservation Number,Reservation Date,Total Price,Table Number,Customer,Buffet Type,Special Request,Status");

            foreach (var reservation in reservations)
            {
                csvContent.AppendLine($"{reservation.ReservationNumber},{reservation.ReservationDate},{reservation.TotalPrice.ToString("F2")},{reservation.TableNumber},{reservation.CustomerFullName},{reservation.BuffetType},{reservation.SpecialRequest},{reservation.ReservationStatus}");
            }

            var byteArray = Encoding.UTF8.GetBytes(csvContent.ToString());
            var stream = new MemoryStream(byteArray);

            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var logParams = new SqlParameter[]
            {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Export all reservations into csv file" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

            return File(stream, "text/csv", csvFileName);
        }
    }
}
