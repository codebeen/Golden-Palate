using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RRS.Data;
using RRS.Models;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RRS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AccountController> logger;
        private readonly IConfiguration configuration;

        public PaymentController(ApplicationDbContext context, ILogger<AccountController> logger, IConfiguration configuration)
        {
            this.context = context;
            this.logger = logger;
            this.configuration = configuration; 
        }

        public string GetAuthenticatedUserEmail()
        {
            return User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.Name)?.Value : null;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var payments = context.PaymentDetails.FromSqlRaw("EXEC GetAllPaymentDetailsFromView").ToList();

            return View("Index", payments);
        }

        [Authorize(Roles = "Admin")]
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


        [Authorize(Roles = "Admin")]
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

            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var logParams = new SqlParameter[]
            {
                new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Export all payments into csv file" },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

            return File(stream, "text/csv", csvFileName);
        }


        [Authorize(Roles = "Admin, Staff")]
        [HttpPost]
        public IActionResult StorePayment(decimal amount, string? description, int reservationId, string modeOfPayment)
        {

            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(authenticatedUserId) || !int.TryParse(authenticatedUserId, out int userId))
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
                    new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId },
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
                    HttpContext.Session.Remove("PayMongoCheckoutId");

                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Created payment" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    TempData["SuccessMessage"] = "Successfully completed the reservation!";
                    return Json(new { success = true, reservationId, redirectUrl = Url.Action("Index", "StaffReservation") });
                }
                else
                {
                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to store payment" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    return Json(new { success = false, message = "Failed to process the payment." });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing payment.");
                return Json(new { success = false, message = "An error occurred while processing the payment." });
            }
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCashlessPayment(int id)
        {
            var secretKey = configuration["PayMongo:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                logger.LogError("PayMongo secret key is missing.");
                return BadRequest("Payment gateway configuration error.");
            }

            var reservation = context.Reservations
                .FromSqlRaw("EXEC GetReservationNumberById @p0", id)
                .AsEnumerable()
                .FirstOrDefault();

            if (reservation == null)
            {
                logger.LogError("Reservation not found with ID: {ReservationId}", id);
                return BadRequest("Reservation not found.");
            }

            string description = $"Payment for {reservation.ReservationNumber}";
            string successUrl = Url.Action("RetrieveCheckoutPaymentMethod", "Payment", new
            {
                amount = reservation.TotalPrice,
                description,
                reservationId = reservation.Id
            }, Request.Scheme);

            string cancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme);

            var options = new RestClientOptions("https://api.paymongo.com/v1/checkout_sessions");
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(secretKey + ":"))}");

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        send_email_receipt = true,
                        show_description = true,
                        show_line_items = true,
                        cancel_url = cancelUrl,
                        description = description,
                        line_items = new[]
                        {
                            new
                            {
                                currency = "PHP",
                                amount = (int)(reservation.TotalPrice * 100), // Convert to centavos
                                name = reservation.ReservationNumber,
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "gcash", "card", "paymaya" },
                        success_url = successUrl,
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            request.AddStringBody(jsonPayload, DataFormat.Json);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                logger.LogError("Failed to create PayMongo checkout session. Status: {StatusCode}, Response: {Response}", response.StatusCode, response.Content);
                return StatusCode((int)response.StatusCode, response.Content);
            }

            var responseData = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var checkoutUrl = responseData.GetProperty("data").GetProperty("attributes").GetProperty("checkout_url").GetString();
            var checkoutId = responseData.GetProperty("data").GetProperty("id").GetString();

            // Store checkout ID for future reference (e.g., session, DB, etc.)
            HttpContext.Session.SetString("PayMongoCheckoutId", checkoutId);

            return Redirect(checkoutUrl);
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpGet]
        public async Task<IActionResult> RetrieveCheckoutPaymentMethod(decimal amount, string description, int reservationId)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            var secretKey = configuration["PayMongo:SecretKey"];
            var checkoutId = HttpContext.Session.GetString("PayMongoCheckoutId");

            if (string.IsNullOrEmpty(checkoutId))
            {
                return BadRequest("Checkout session ID is missing.");
            }

            var options = new RestClientOptions($"https://api.paymongo.com/v1/checkout_sessions/{checkoutId}");
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(secretKey + ":"))}");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                logger.LogError("Failed to retrieve PayMongo checkout session. Status: {StatusCode}, Response: {Response}", response.StatusCode, response.Content);
                return StatusCode((int)response.StatusCode, response.Content);
            }

            var responseData = JsonSerializer.Deserialize<JsonElement>(response.Content);

            try
            {
                var attributes = responseData.GetProperty("data").GetProperty("attributes");
                var paymentsArray = attributes.GetProperty("payments");

                if (paymentsArray.GetArrayLength() > 0)
                {
                    var paymentAttributes = paymentsArray[0].GetProperty("attributes");
                    var paymentStatus = paymentAttributes.GetProperty("status").GetString();

                    if (paymentStatus == "paid")
                    {
                        var modeOfPayment = attributes.GetProperty("payment_method_used").GetString();

                        ViewData["Amount"] = amount;
                        ViewData["Description"] = description;
                        ViewData["ReservationId"] = reservationId;
                        ViewData["ModeOfPayment"] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modeOfPayment.ToLower());

                        var logParams = new SqlParameter[]
                        {
                            new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Process cashless payment" },
                            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                        };
                        context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                        return View("RedirectToStorePayment");
                    }
                    else
                    {
                        logger.LogWarning("Payment status is not 'paid'. Status: {PaymentStatus}", paymentStatus);
                        return BadRequest($"Payment status is not 'paid'. Status: {paymentStatus}");
                    }
                }
                else
                {
                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to process cashless payment" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    logger.LogWarning("No payments found for this checkout session.");
                    return BadRequest("No payments found for this checkout session.");
                }
            }
            catch (Exception ex)
            {
                var logParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to process cashless payment" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                logger.LogError(ex, "Exception occurred while retrieving payment method.");
                return StatusCode(500, "An error occurred while processing the payment.");
            }
        }

    }
}
