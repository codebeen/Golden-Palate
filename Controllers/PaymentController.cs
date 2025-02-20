using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using System.Text;

namespace RRS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AccountController> logger;

        public PaymentController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public string GetAuthenticatedUserEmail()
        {
            return HttpContext.Session.GetString("UserEmail");
        }

        public IActionResult Index()
        {
            var payments = context.PaymentDetails.FromSqlRaw("EXEC GetAllPaymentDetailsFromView").ToList();

            return View("Index", payments);
        }


        public IActionResult ViewPaymentDetails(int id)
        {
            var getSpecificPayment = context.PaymentDetails.FromSqlRaw("GetPaymentDetailsById @p0", id).AsEnumerable().FirstOrDefault();

            if (getSpecificPayment != null)
            {
                TempData["ErrorMessage"] = "Payment not found";

                return RedirectToAction("Index");
            }

            return PartialView("PaymentDetails", getSpecificPayment);
        }

        public ActionResult Export()
        {
            var payments = context.PaymentDetails.FromSqlRaw("EXEC GetAllPaymentDetailsFromView").ToList();

            var csvFileName = $"payments_{DateTime.Now:yyyy-MM-dd}.csv";
            var csvContent = new StringBuilder();

            // Add CSV headers
            csvContent.AppendLine("Customer Name,Reservation Number,Amount,Description,Issued By,Payment Method,Date Created");

            foreach (var payment in payments)
            {
                csvContent.AppendLine($"{payment.CustomerFullName},{payment.ReservationNumber},{payment.Amount},{payment.Description},{payment.UserFullName},{payment.ModeOfPayment},{payment.CreatedAt.ToString("MMMM dd, yyyy")}");
            }

            var byteArray = Encoding.UTF8.GetBytes(csvContent.ToString());
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/csv", csvFileName);
        }


        [HttpPost]
        public IActionResult StorePayment(decimal amount, string? description, int reservationId, string modeOfPayment)
        {
            var authenticatedEmail = GetAuthenticatedUserEmail();
            var authenticatedUser = context.Users.FromSqlRaw("EXEC GetUserByEmail @p0", authenticatedEmail).AsEnumerable().FirstOrDefault();

            if (authenticatedUser == null)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Amount", System.Data.SqlDbType.Decimal) { Value = amount },
                    new SqlParameter("@Description", System.Data.SqlDbType.VarChar) { Value = (object?)description ?? DBNull.Value },
                    new SqlParameter("@ReservationId", System.Data.SqlDbType.Int) { Value = reservationId },
                    new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = authenticatedUser.Id },
                    new SqlParameter("@ModeOfPayment", System.Data.SqlDbType.VarChar) { Value = modeOfPayment }
                };

                var result = context.Database.ExecuteSqlRaw("EXEC CreatePayment @Amount, @Description, @ReservationId, @UserId, @ModeOfPayment", parameters);

                if (result > 0)
                {
                    // Update reservation status to 'Completed'
                    var statusParameters = new[]
                    {
                        new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = reservationId },
                        new SqlParameter("@Status", System.Data.SqlDbType.VarChar) { Value = "Completed" }
                    };

                    context.Database.ExecuteSqlRaw("EXEC UpdateReservationStatus @Id, @Status", statusParameters);

                    return Json(new { success = true, reservationId });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process the payment." });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing payment.");
                return Json(new { success = false, message = "An error occurred while processing the payment." });
            }
        }




        //[HttpPost]
        //public IActionResult ProcessCashlessPayment(Payment payment)
        //{

        //}
    }
}
