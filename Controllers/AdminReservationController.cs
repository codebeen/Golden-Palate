using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models.ViewModels;

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
            var getSpecificReservation = context.ReservationDetails.FromSqlRaw($"GetReservationDetailsById {id}").AsEnumerable().FirstOrDefault();

            if (getSpecificReservation != null)
            {
                TempData["ErrorMessage"] = "Reservation not found";

                return RedirectToAction("Index");
            }

            return PartialView("ReservationDetails", getSpecificReservation);
        }
    }
}
