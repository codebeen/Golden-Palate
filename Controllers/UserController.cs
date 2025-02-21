using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Crypto.Generators;
using RRS.Data;
using RRS.Models;
using BCrypt.Net;
using RRS.Services;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace RRS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly EmailService _emailService;
        private readonly ILogger<UserController> logger;

        public UserController(ApplicationDbContext context, EmailService emailService, ILogger<UserController> logger)
        {
            this.context = context;
            _emailService = emailService;
            this.logger = logger;
        }


        public IActionResult Index()
        {
            var users = context.Users.FromSqlRaw("GetAllUsers").ToList();
            return View("Index", users);
        }

        public IActionResult Create()
        {
            User user = new();

            return PartialView("AddUserModal", user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            try
            {
                // Generate random password
                var password = GenerateSecurePassword(12);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                user.Password = hashedPassword;

                // Prepare SQL Parameters
                var userParameters = new[]
                {
                    new SqlParameter("@FirstName", SqlDbType.VarChar) { Value = user.FirstName },
                    new SqlParameter("@LastName", SqlDbType.VarChar) { Value = user.LastName },
                    new SqlParameter("@Email", SqlDbType.VarChar) { Value = user.Email },
                    new SqlParameter("@Password", SqlDbType.VarChar) { Value = user.Password },
                    new SqlParameter("@Role", SqlDbType.VarChar) { Value = user.Role },
                };

                // Execute Stored Procedure
                var createUserResult = await context.Database.ExecuteSqlRawAsync(
                    "EXEC RegisterUser @FirstName, @LastName, @Email, @Password, @Role", userParameters);

                if (createUserResult == 1)
                {
                    string subject = "Welcome to Golden Palate - Your Login Credentials";
                    string body = $@"
                <h2>Welcome, {user.FirstName} {user.LastName}!</h2>
                <p>Your account has been successfully created. You can now log in to the Golden Palate using the credentials below:</p>
                <ul>
                    <li><strong>Email:</strong> {user.Email}</li>
                    <li><strong>Password:</strong> {password}</li>
                </ul>
                <p>For security reasons, please change your password upon logging in for the first time.</p>
                <p>If you have any questions or need assistance, feel free to contact us.</p>
                <p>Best regards,<br>Golden Palate Team</p>
            ";

                    await _emailService.SendEmailAsync(user.Email, subject, body);

                    // log
                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Create user" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    TempData["SuccessMessage"] = "User added successfully. Their login credentials have been sent to the respective email address.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // log
                var error1logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to create user" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", error1logParams);

                Console.WriteLine($"Error adding user: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to add the user. Please check the input and try again.";
                return RedirectToAction("Index");
            }

            // log
            var errorlogParams = new SqlParameter[]
            {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to create user" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

            TempData["ErrorMessage"] = "Failed to add the user. Please check the input and try again.";
            return RedirectToAction("Index");
        }


        private string GenerateSecurePassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            Random random = new();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public IActionResult Edit(int id)
        {
            try
            {
                var userToEdit = context.Users.FromSqlRaw("GetUserById @p0", id).AsEnumerable().FirstOrDefault();

                return PartialView("EditUserModal", userToEdit);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Edit(User user)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var existingUser = context.Users.FromSqlRaw($"GetUserById @p0", user.Id).AsEnumerable().FirstOrDefault();

            if (existingUser != null)
            {
                // Check if there are any changes in the user details
                if (user.Role == existingUser.Role)
                {
                    TempData["InformationMessage"] = "No changes have been made";
                    return RedirectToAction("Index");
                }

                existingUser.Role = user.Role;

                var userParameter = new SqlParameter[]
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = existingUser.Id },
                    new SqlParameter("@Role", SqlDbType.VarChar) { Value = existingUser.Role },
                };

                var updateUserResult = context.Database.ExecuteSqlRaw("UpdateUser @Id, @Role", userParameter);

                if (updateUserResult > 0)
                {
                    // log
                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Update user details" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    TempData["SuccessMessage"] = "User details updated successfully";
                    return RedirectToAction("Index");
                }

                // log
                var errorlogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to update user details" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                TempData["ErrorMessage"] = "Failed to update user details";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            try
            {
                var userToDelete = context.Users.FromSqlRaw("GetUserById @p0", id).AsEnumerable().FirstOrDefault();

                return PartialView("DeleteUserModal", userToDelete);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Delete(User user)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            try
            {
                var existingUser = context.Users.FromSqlRaw("GetUserById @p0", user.Id).AsEnumerable().FirstOrDefault();

                if (existingUser != null)
                {
                    // Create the parameter array with proper SqlParameter setup
                    var parameter = new SqlParameter[]
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@Id",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = user.Id
                        },
                    };

                    // Execute the stored procedure with the parameter array
                    var deleteUserResult = context.Database.ExecuteSqlRaw("EXEC DeleteUser @Id", parameter);

                    if (deleteUserResult > 0)
                    {
                        // log
                        var logParams = new SqlParameter[]
                        {
                            new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Delete a user" },
                            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                        };
                        context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                        TempData["SuccessMessage"] = "User successfully deleted.";
                        return RedirectToAction("Index");
                    }

                    // log
                    var errorlogParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to delete user" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                    TempData["ErrorMessage"] = "Failed to delete this user.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // log
                var errorlogParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to delete user" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "Failed to delete this user.";
                return RedirectToAction("Index");
            }
        }


        public ActionResult Export()
        {
            var users = context.Users.FromSqlRaw("GetAllUsers").ToList();

            var csvFileName = $"users_{DateTime.Now:yyyy-MM-dd}.csv";
            var csvContent = new StringBuilder();

            // Add CSV headers
            csvContent.AppendLine("Full Name,Email,Role,Status");

            foreach (var user in users)
            {
                csvContent.AppendLine($"{user.FirstName} {user.LastName},{user.Email},{user.Role},{user.Status}");
            }

            var byteArray = Encoding.UTF8.GetBytes(csvContent.ToString());
            var stream = new MemoryStream(byteArray);

            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var logParams = new SqlParameter[]
            {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Export all user details into csv file" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

            return File(stream, "text/csv", csvFileName);
        }
    }
}
