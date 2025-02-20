using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RRS.Data;
using RRS.Models;
using System.Globalization;
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
                    HttpContext.Session.Remove("PayMongoCheckoutId");

                    TempData["SuccessMessage"] = "Successfully completed the reservation!";
                    return Json(new { success = true, reservationId, redirectUrl = Url.Action("Index", "StaffReservation") });
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


        [HttpGet]
        public async Task<IActionResult> RetrieveCheckoutPaymentMethod(decimal amount, string description, int reservationId)
        {
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
                    logger.LogWarning("No payments found for this checkout session.");
                    return BadRequest("No payments found for this checkout session.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while retrieving payment method.");
                return StatusCode(500, "An error occurred while processing the payment.");
            }
        }

    }
}
