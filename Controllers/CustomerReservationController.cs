using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using RRS.Models.ViewModels;
using System.Data;

namespace RRS.Controllers
{
    public class CustomerReservationController : Controller
    {
        private readonly ApplicationDbContext context;

        public CustomerReservationController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult MyReservations()
        {
            return View();
        }

        public IActionResult DisplayBuffets()
        {
            return View("SelectBuffet");
        }

        // Breakfast Buffet
        public IActionResult DisplayTablesForBreakfastBuffet()
        {
            try
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.BuffetPrice = 1680;
                tableViewModel.BuffetName = "Breakfast";

                // get available tables for today and do not have reservation for 8 am to 9 am
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday_8AMto9AM").ToList();

                return View("SelectTableForBreakfast", tableViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("DisplayBuffets");
            }
        }

        // Lunch Buffet
        public IActionResult DisplayTablesForLunchBuffet()
        {
            try
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.BuffetPrice = 2800;
                tableViewModel.BuffetName = "Lunch";

                // get available tables for today and do not have reservation for 12 pm to 2pm
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday_12PMto2PM").ToList();

                return View("SelectTableForBreakfast", tableViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("DisplayBuffets");
            }
        }

        // Dinner Buffet
        public IActionResult DisplayTablesForDinnerBuffet()
        {
            try
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.Table = new Table();
                tableViewModel.BuffetPrice = 4200;
                tableViewModel.BuffetName = "Dinner";

                // get available tables for today and do not have reservation for 5 pm to 7pm
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday_5PMto7PM").ToList();

                return View("SelectTableForBreakfast", tableViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("DisplayBuffets");
            }
        }

        // Display Reservation Form
        [HttpPost]
        public IActionResult DisplayReservationForm(int tableId, int tableNumber, decimal price, decimal buffetPrice, DateOnly date, TimeOnly time, string buffetType)
        {
            try
            {
                // Validate required inputs
                if (date == null || time == null)
                {
                    TempData["ErrorMessage"] = "Reservation date and time are required.";
                    return RedirectToAction("DisplayBuffets");
                }

                // Create new reservation instance
                Reservation reservation = new Reservation
                {
                    Table = new Table
                    {
                        TableNumber = tableNumber,
                        Price = price
                    },
                    Customer = new Customer(),
                    TableId = tableId,
                    BuffetType = buffetType,
                    ReservationDate = date,
                    ReservationTime = time,
                    TotalPrice = price + buffetPrice
                };

                Console.WriteLine($"Table Reserved: {reservation.Table.TableNumber}, Date: {reservation.ReservationDate}, Time: {reservation.ReservationTime}");

                // Return reservation form view with reservation details
                return View("ReservationForm", reservation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                TempData["ErrorMessage"] = "An error occurred while processing your reservation.";
                return RedirectToAction("DisplayBuffets");
            }
        }


        [HttpPost]
        public IActionResult CreateReservation(Reservation reservation)
        {
            try
            {
                // create customer
                Customer customer = new Customer
                {
                    FirstName = reservation.Customer.FirstName,
                    LastName = reservation.Customer.LastName,
                    PhoneNumber = reservation.Customer.PhoneNumber,
                    Email = reservation.Customer.Email
                };

                var customerParameter = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", SqlDbType.VarChar) { Value = customer.FirstName },
                    new SqlParameter("@LastName", SqlDbType.VarChar) { Value = customer.LastName },
                    new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { Value = customer.PhoneNumber },
                    new SqlParameter("@Email", SqlDbType.VarChar) { Value = customer.Email }
                };

                var createdCustomerId = context.Database.ExecuteSqlRaw($"Exec CreateCustomer @FirstName, @LastName, @PhoneNumber, @Email", customerParameter);

                if (createdCustomerId == 0)
                {
                    TempData["ErrorMessage"] = "Unable to add customer.";
                    return View("DisplayBuffet");
                }

                var reservationParameter = new SqlParameter[]
                {
                    new SqlParameter("@ReservationDate", SqlDbType.Date) { Value = reservation.ReservationDate },
                    new SqlParameter("@ReservationTime", SqlDbType.Time) { Value = reservation.ReservationTime },
                    new SqlParameter("@TotalPrice", SqlDbType.Decimal) { Value = reservation.TotalPrice },
                    new SqlParameter("@BuffetType", SqlDbType.VarChar) { Value = reservation.BuffetType },
                    new SqlParameter("@SpecialRequest", SqlDbType.VarChar) { Value = string.IsNullOrEmpty(reservation.SpecialRequest) ? DBNull.Value : reservation.SpecialRequest },
                    new SqlParameter("@TableId", SqlDbType.Int) { Value = reservation.TableId },
                    new SqlParameter("@CustomerId", SqlDbType.Int) { Value = createdCustomerId }
                };

                var createReservationResult = context.Database.ExecuteSqlRaw($"Exec CreateReservation @ReservationDate, @ReservationTime, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId", reservationParameter);

                if (createReservationResult == 1)  // Checking if 1 row was affected
                {
                    TempData["SuccessMessage"] = "Reservation created successfully.";
                    return RedirectToAction("DisplayBuffets");
                }

                TempData["ErrorMessage"] = "Failed to create reservation.";
                return RedirectToAction("DisplayBuffets");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occurred while creating the reservation.";
                return RedirectToAction("DisplayBuffets");
            }
        }

    }
}
