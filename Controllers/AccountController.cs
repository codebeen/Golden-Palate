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
				return View("Login", user);  // Specify the correct view name if necessary
			}

			// hash the password first
			user.Password = HashPassword(user.Password);
			_logger.LogInformation("Password Hashed");

			var userCountParam = new SqlParameter("@UserCount", System.Data.SqlDbType.Int) { Direction = ParameterDirection.Output };
			var roleParam = new SqlParameter("@Role", System.Data.SqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };

			_context.Database.ExecuteSqlRaw("EXEC LoginUser @Email, @Password, @UserCount OUTPUT, @Role OUTPUT",
				new SqlParameter("@Email", user.Email),
				new SqlParameter("@Password", user.Password),
				userCountParam,
				roleParam);

			int userCount = (int?)userCountParam.Value ?? 0; // Handle potential null
			string role = userCount > 0 ? (string)roleParam.Value : null; // Only get the role if user exists

			if (userCount == 1)  // Check if exactly one matching user is found
			{
				_logger.LogInformation("Logged in");
				// Login successful, store the role and other user data if necessary
				//TempData["SuccessMessage"] = "Login successful!";

				//// Optionally, you can store the user's role in session or authentication token
				////HttpContext.Session.SetString("UserRole", role);  // Example of storing role in session
				//												  // Store only readily available data in session
				//HttpContext.Session.SetString("UserEmail", user.Email); // Store the email
				//HttpContext.Session.SetString("UserRole", role); // Store the role
				//HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString());

				// Redirect to the appropriate page based on the role
				if (role == "Admin")
				{
					return RedirectToAction("Index", "Dashboard"); // Correct way to redirect
				}
				else if (role == "Staff")
				{
					return RedirectToAction("Dashboard", "StaffReservation"); // Correct way to redirect
				}
			}

			_logger.LogInformation("No matching users{Email}{Password}", user.Email, user.Password);
			// Invalid login attempt
			ModelState.AddModelError("", "Invalid credentials. Please try again.");
			return View("Login", user);  // Specify the correct view name if necessary
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
				user.Password = HashPassword(user.Password);
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
			// Debug: Log logout attempt
			_logger.LogInformation("Logging out user.");

			// Clear the session
			//HttpContext.Session.Clear();  // Clears all session data

			// Optionally, you can also clear authentication cookies if using cookie-based authentication:
			// _signInManager.SignOutAsync(); // if you are using ASP.NET Identity

			// Log the user out
			_logger.LogInformation("User logged out successfully.");

			// Redirect to the Login page or Home page
			return RedirectToAction("Login", "Account");  // Adjust to your needs
		}

		private string HashPassword(string password)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return BitConverter.ToString(bytes).Replace("-", "").ToLower();
			}
		}

		//public class LoginResult
		//{
		//	public int UserCount { get; set; }
		//	public string Role { get; set; }
		//}

	}
}
