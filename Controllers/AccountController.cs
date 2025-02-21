using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using System.Security.Claims;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;

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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) // Check if user is already logged in
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (role == "Staff")
                {
                    return RedirectToAction("Dashboard", "StaffReservation");
                }

                return RedirectToAction("Index", "Home"); // Fallback for other roles
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginUser(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View("Login", user);
            }

            try
            {
                var emailParam = new SqlParameter("@Email", SqlDbType.VarChar) { Value = user.Email };

                var retrievedUser = _context.Users
                    .FromSqlRaw("EXEC LoginUser @Email", emailParam)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (retrievedUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, retrievedUser.Password))
                {
                    // Audit log for failed login
                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed login attempt" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = DBNull.Value },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    ModelState.AddModelError("", "Invalid email or password.");
                    return View("Login", user);
                }

                if (retrievedUser.Status.ToLower() == "inactive")
                {
                    var status = "Active";
                    _context.Database.ExecuteSqlRaw("EXEC UpdateUserStatus @p0, @p1", retrievedUser.Id, status);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, retrievedUser.Email),
                    new Claim(ClaimTypes.NameIdentifier, retrievedUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, retrievedUser.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Audit log for successful login
                var successLogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "User login" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = retrievedUser.Id },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                };
                await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", successLogParams);

                return retrievedUser.Role == "Admin" ? RedirectToAction("Index", "Dashboard") : RedirectToAction("Dashboard", "StaffReservation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");

                // Audit log for login error
                var errorLogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed login attempt" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = DBNull.Value },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", errorLogParams);

                return View("Login", user);
            }
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public IActionResult Register()
        //{
        //    return View();
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public IActionResult RegisterUser(User user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View("Register", user);
        //    }

        //    var existingUser = _context.Users
        //        .FromSqlRaw("SELECT * FROM Users WHERE Email = {0}", user.Email)
        //        .AsEnumerable()
        //        .FirstOrDefault();

        //    if (existingUser != null)
        //    {
        //        ModelState.AddModelError("Email", "This email is already registered.");
        //        return View("Register", user);
        //    }

        //    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        //    int result = _context.Database.ExecuteSqlRaw("EXEC RegisterUser @p0, @p1, @p2, @p3", user.FirstName, user.LastName, user.Email, user.Password);

        //    if (result == 0)
        //    {
        //        ModelState.AddModelError("", "Registration failed. Please try again.");
        //        return View("Register", user);
        //    }

        //    TempData["SuccessMessage"] = "Registration successful!";
        //    return RedirectToAction("Login");
        //}

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LogoutUser()
        {
            try
            {
                // Retrieve UserId securely from authentication claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid or missing UserId in claims.");
                    return RedirectToAction("Login");
                }

                // Audit log for user logout
                var logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "User logout" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                };
                await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                // Sign out user
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User {UserId} logged out successfully.", userId);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout.");

                // Log failed logout attempt
                var errorLogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to logout" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = DBNull.Value },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", errorLogParams);

                return RedirectToAction("Login");
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> AccessDenied()
        {
            try
            {
                // Retrieve UserId securely from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int id) ? 0 : id;

                // Log unauthorized access attempt
                var logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Access denied attempt" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                await _context.Database.ExecuteSqlRawAsync("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                _logger.LogWarning("User {UserId} attempted to access {Page} but was denied.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging access denied.");
            }

            return View("AccessDenied");
        }


        [AllowAnonymous]
        public IActionResult ChangePassword()
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userIdPassword))
            {
                _logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var user = _context.Users.FromSqlRaw("EXEC GetUserById @p0", userIdPassword).AsEnumerable().FirstOrDefault();

            return View("ChangePassword", user);
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult UpdatePassword(User user)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine(user.Id);
            Console.WriteLine(user.Password);
            Console.WriteLine(userRole);

            var userId = user.Id;
            var password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            Console.WriteLine(password);

            var userParam = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@NewPassword", SqlDbType.VarChar) { Value = password }
            };

            var result = _context.Database.ExecuteSqlRaw("EXEC UpdatePassword @UserId, @NewPassword", userParam);

            if (result > 0)
            {
                var logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Changed password" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                };
                _context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                TempData["SuccessMessage"] = "Successfully changed your password.";

                return userRole == "Admin" ? RedirectToAction("Index", "Dashboard") : RedirectToAction("Dashboard", "StaffReservation");
            }

            var errorlogParams = new SqlParameter[]
            {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to change password" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
            };
            _context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

            TempData["ErrorMessage"] = "Failed to change your password.";
            return View("ChangePassword", user);
        }

    }
}
