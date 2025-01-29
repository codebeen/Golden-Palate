using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class AdminReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
