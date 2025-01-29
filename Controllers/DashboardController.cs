using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
