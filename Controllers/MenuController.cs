using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
