using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models.ViewModels;
using System.Text;

namespace RRS.Controllers
{
    public class AdminReservationController : Controller
    {
        private readonly ApplicationDbContext context;

        public AdminReservationController(ApplicationDbContext context)
        {
            this.context = context;
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

            return File(stream, "text/csv", csvFileName);
        }
    }
}
