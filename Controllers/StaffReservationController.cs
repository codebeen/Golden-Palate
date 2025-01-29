using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class StaffReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
