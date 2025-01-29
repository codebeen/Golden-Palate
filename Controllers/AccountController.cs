using Microsoft.AspNetCore.Mvc;
using RRS.Models;

namespace RRS.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            User user = new User();

            return View("Login");
        }

        [HttpPost]
        public IActionResult LoginUser(User user)
        {
            return View(user);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public IActionResult RegisterUser(User user)
        {
            return View(user);
        }

        [HttpPost]
        public IActionResult LogoutUser(User user)
        {
            return View(user);
        }
    }
}
