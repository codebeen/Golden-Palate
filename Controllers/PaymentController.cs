using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;

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
            return View();
        }


        [HttpPost]
        public IActionResult StorePayment(Payment payment)
        {
            var authenticatedEmail = GetAuthenticatedUserEmail();

            try
            {
                var paymentParameter = new SqlParameter[]
                {
                    new SqlParameter() { ParameterName = "@Amount", SqlDbType = System.Data.SqlDbType.Decimal, Value = payment.Amount },
                    new SqlParameter() { ParameterName = "@Description", SqlDbType = System.Data.SqlDbType.VarChar, Value = payment.Description },
                    new SqlParameter() { ParameterName = "@ReservationId", SqlDbType = System.Data.SqlDbType.Int, Value = payment.ReservationId },
                    new SqlParameter() { ParameterName = "@UserId", SqlDbType = System.Data.SqlDbType.Int, Value = payment.UserId},
                    new SqlParameter() { ParameterName = "@ModeOfPayment", SqlDbType = System.Data.SqlDbType.VarChar, Value = payment.ModeOfPayment},
                };


                var result = context.Database.ExecuteSqlRaw("Exec CreatePayment @Amount, @Description, @ReservationId, @UserId, @ModeOfPayment", paymentParameter);

                if (result > 0)
                {
                    TempData["SuccessPayment"] = "Successfully processed the payment.";
                    return RedirectToAction("Index", "StaffReservation");
                }

                TempData["ErrorMessage"] = "Failed to process the payment.";
                return RedirectToAction("Index", "StaffReservation");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "Failed to process the payment.";
                return RedirectToAction("Index", "StaffReservation");
            }
        }


        //[HttpPost]
        //public IActionResult ProcessCashlessPayment(Payment payment)
        //{

        //}
    }
}
