using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RRS.Controllers
{
    public class AccountController : Controller
    {
		private readonly ApplicationDbContext _context;
		private readonly ILogger<AccountController> _logger;

		public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[HttpGet]
        public IActionResult Login()
        {
            User user = new User();

            return View("Login");
        }

        [HttpPost]
        public IActionResult LoginUser(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                _logger.LogInformation("Email and Password empty {Email} {Password}", user.Email, user.Password);
                ModelState.AddModelError("", "Email and password are required.");
                return View("Login", user);
            }

            try
            {
                var emailParam = new SqlParameter("@Email", SqlDbType.VarChar) { Value = user.Email };

                // Assuming your SP returns User details including hashed password and role
                var retrievedUser = _context.Users
                    .FromSqlRaw("EXEC LoginUser @Email", emailParam)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (retrievedUser == null)
                {
                    _logger.LogInformation("No user found with email: {Email}", user.Email);
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View("Login", user);
                }

                // Check password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(user.Password, retrievedUser.Password);

                if (!isPasswordValid)
                {
                    _logger.LogInformation("Invalid password for user: {Email}", user.Email);
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View("Login", user);
                }

                // Login successful
                _logger.LogInformation("User logged in: {Email}", user.Email);

                // Example: Store email & role in session (or use claims)
                HttpContext.Session.SetString("UserEmail", retrievedUser.Email);
                HttpContext.Session.SetString("UserRole", retrievedUser.Role);

                Console.WriteLine(GetAuthenticatedUserEmail());

                if (retrievedUser.Status.ToLower() == "inactive")
                {
                    var status = "Active";

                    var resultQuery = _context.Database.ExecuteSqlRaw("EXEC UpdateUserStatus @p0, @p1", retrievedUser.Id, status);
                }

                if (retrievedUser.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (retrievedUser.Role == "Staff")
                {
                    return RedirectToAction("Dashboard", "StaffReservation");
                }

                // Default redirect
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("Login", user);
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
			User user = new User();

            return View("Register");
        }

		[HttpPost]
		public IActionResult RegisterUser(User user)
		{
			// Assign default 'admin' role before validation
			//// Remove to default to 'staff'
			//if (string.IsNullOrEmpty(user.Role))
			//{
			//	user.Role = "Admin";  // Default role
			//	ModelState.Clear();  // Clear validation errors
			//	TryValidateModel(user);  // Revalidate after setting Role
			//	_logger.LogInformation("User rOLE: {Role}", user.Role);
			//}

			//_logger.LogInformation("User rOLE outside: {Role}", user.Role);

			try
			{
				_logger.LogInformation("RegisterUser called with Email: {Email}", user.Email);

				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values.SelectMany(v => v.Errors)
												  .Select(e => e.ErrorMessage)
												  .ToList();

					_logger.LogWarning("Model state is invalid. Errors: {Errors}", string.Join(" | ", errors));

					return View("Register", user);
				}

				// Check if email already exists
				var existingUser = _context.Users
					.FromSqlRaw("SELECT * FROM Users WHERE Email = {0}", user.Email)
					.AsEnumerable()
					.FirstOrDefault();

				if (existingUser != null)
				{
					ModelState.AddModelError("Email", "This email is already registered.");
					_logger.LogWarning("Email already exists: {Email}", user.Email);
					return View("Register", user);
				}

				// Hash the password
				user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
				_logger.LogInformation("Password hashed successfully.");

				// Execute stored procedure
				int result = _context.Database.ExecuteSqlRaw("EXEC RegisterUser @p0, @p1, @p2, @p3",
					user.FirstName, user.LastName, user.Email, user.Password);

				_logger.LogInformation("Stored procedure executed, affected rows: {Result}", result);

				if (result == 0)
				{
					ModelState.AddModelError("", "Registration failed. Please try again.");
					_logger.LogWarning("Stored procedure did not insert a user.");
					return View("Register", user);
				}

				TempData["SuccessMessage"] = "Registration successful!";
				_logger.LogInformation("Registration successful, redirecting to Login.");

				return RedirectToAction("Login");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during registration.");
				ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
				return View("Register", user);
			}
		}

		[HttpGet]
        public IActionResult LogoutUser()
        {
            _logger.LogInformation("Logging out user.");
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out successfully.");
            return RedirectToAction("Login", "Account");
        }


        public string GetAuthenticatedUserEmail()
        {
            return HttpContext.Session.GetString("UserEmail");
        }

        public string GetAuthenticatedUserRole()
        {
            return HttpContext.Session.GetString("UserRole");
        }

        //private string HashPassword(string password)
        //{
        //	using (SHA256 sha256 = SHA256.Create())
        //	{
        //		byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        //		return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        //	}
        //}

        //public class LoginResult
        //{
        //	public int UserCount { get; set; }
        //	public string Role { get; set; }
        //}

    }
}
