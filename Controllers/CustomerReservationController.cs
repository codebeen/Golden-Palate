using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class CustomerReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
