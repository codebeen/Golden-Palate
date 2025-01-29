using Microsoft.AspNetCore.Mvc;

namespace RRS.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
