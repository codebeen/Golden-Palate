using Microsoft.AspNetCore.Mvc;
using RRS.Models;
using System.Diagnostics;

namespace RRS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Menu()
        {
            return View("~/Views/Home/Menu.cshtml");
        }

        public IActionResult Customers()
        {
            return View("~/Views/Home/Customers.cshtml");
        }

        public IActionResult AboutUs()
        {
            return View("~/Views/Home/AboutUs.cshtml");
        }

        public IActionResult ContactUs()
        {
            return View("~/Views/Home/ContactUs.cshtml");
        }


        public IActionResult Header()
        {
            return View("~/Views/Home/Header.cshtml");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
