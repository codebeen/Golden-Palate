using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RRS.Data;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace RRS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AuditLogController> logger;

        public AuditLogController(ApplicationDbContext context, ILogger<AuditLogController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var auditLogs = context.AuditLogDetails.FromSqlRaw("EXEC GetAuditLogDetailsFromView").ToList();

            return View("Index", auditLogs);
        }

        public IActionResult ViewAuditLogDetails(int id)
        {
            var getSpecificLog = context.AuditLogDetails.FromSqlRaw("GetAuditLogById @p0", id).AsEnumerable().FirstOrDefault();

            if (getSpecificLog != null)
            {
                TempData["ErrorMessage"] = "Log not found";

                return RedirectToAction("Index");
            }

            return PartialView("AuditLogDetails", getSpecificLog);
        }

        public ActionResult Export()
        {
            var auditLogs = context.AuditLogDetails.FromSqlRaw("EXEC GetAuditLogDetailsFromView").ToList();

            var csvFileName = $"auditLogs_{DateTime.Now:yyyy-MM-dd}.csv";
            var csvContent = new StringBuilder();

            // Add CSV headers
            csvContent.AppendLine("Timestamp,UserFullname,Email,Role,Activity,Status");

            foreach (var log in auditLogs)
            {
                string fullName = (log.FirstName == "System" && log.LastName == "System") ? "System" : $"{log.FirstName} {log.LastName}";

                csvContent.AppendLine($"{log.CreatedDate},{fullName},{log.Email},{log.UserRole},{log.Activity},{log.Status}");
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
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Export all audit logs into csv file" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

            return File(stream, "text/csv", csvFileName);
        }
    }
}
