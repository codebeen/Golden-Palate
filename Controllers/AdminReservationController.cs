using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRS.Data;

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
            var reservationDetails = context.ReservationDetails.FromSqlRaw("EXEC GetReservationDetails").ToList();

            // Pass the correct model to the view.
            return View("Index", reservationDetails);  // Ensure it's IEnumerable<ReservationDetails>
        }


    }
}
