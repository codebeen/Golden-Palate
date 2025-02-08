using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using RRS.Models.ViewModels;
using RRS.Services;
using System.Data;
using System.Net.Mail;

namespace RRS.Controllers
{
    public class CustomerReservationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly EmailService _emailService;

        public CustomerReservationController(ApplicationDbContext context, EmailService emailService)
        {
            this.context = context;
            _emailService = emailService;
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
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                return View("SelectTable", tableViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "Unable to display tables";
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
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                return View("SelectTable", tableViewModel);
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
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                return View("SelectTable", tableViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction("DisplayBuffets");
            }
        }

        // Display Reservation Form
        [HttpPost]
        public IActionResult DisplayReservationForm(int tableId, int tableNumber, decimal price, decimal buffetPrice, DateOnly date, string buffetType, int seatingCapacity)
        {
            try
            {
                // Validate required inputs
                if (date == null)
                {
                    TempData["ErrorMessage"] = "Reservation date is required.";
                    return RedirectToAction("DisplayBuffets");
                }

                // Create new reservation instance
                Reservation reservation = new Reservation
                {
                    Table = new Table
                    {
                        TableNumber = tableNumber,
                        Price = price,
                        SeatingCapacity = seatingCapacity,
                    },
                    Customer = new Customer(),
                    TableId = tableId,
                    BuffetType = buffetType,
                    ReservationDate = date,
                    TotalPrice = price + buffetPrice
                };

                Console.WriteLine($"Table Reserved: {reservation.Table.TableNumber}, Date: {reservation.ReservationDate}");

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

        public IActionResult ReserveTable(string selectedDate, string buffetType)
        {
            // Make sure selectedDate is parsed into the correct DateTime format
            DateTime reservationDate;
            if (!DateTime.TryParse(selectedDate, out reservationDate))
            {
                // Handle invalid date input (you could return an error message or default value)
                reservationDate = DateTime.Today; // Default to today's date if invalid
            }

            // Use parameterized query to call the stored procedure
            var reservedTables = context.Reservations
                                  .FromSqlRaw("EXEC GetReservedTables @ReservationDate = {0}, @BuffetType = {1}", reservationDate.Date, buffetType)
                                  .AsEnumerable()
                                  .Select(r => r.TableId)
                                  .ToList();

            // Verify the results of the query and make sure ReservedTableIds is populated
            Console.WriteLine("ReservedTableIds count: " + (reservedTables?.Count ?? 0));

            var viewModel = new TableViewModel
            {
                ReservedTableIds = reservedTables ?? new List<int>()
            };

            // check the buffet type 
            if (buffetType == "Breakfast")
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.BuffetPrice = 1680;
                tableViewModel.BuffetName = "Breakfast";
                tableViewModel.ReservedTableIds = reservedTables ?? new List<int>();

                // get available tables for today and do not have reservation for 8 am to 9 am
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                // Verify the results of the query and make sure ReservedTableIds is populated
                Console.WriteLine("ReservedTableIds count: " + (reservedTables?.Count ?? 0));
                TempData["SelectedDate"] = selectedDate;
                return View("SelectTable", tableViewModel);
            }

            if (buffetType == "Lunch")
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.BuffetPrice = 2800;
                tableViewModel.BuffetName = "Lunch";
                tableViewModel.ReservedTableIds = reservedTables ?? new List<int>();

                // get available tables for today and do not have reservation for 12 pm to 2pm
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                Console.WriteLine("ReservedTableIds count: " + (reservedTables?.Count ?? 0));
                TempData["SelectedDate"] = selectedDate;
                return View("SelectTable", tableViewModel);
            }

            if (buffetType == "Dinner")
            {
                TableViewModel tableViewModel = new TableViewModel();
                tableViewModel.Reservation = new Reservation();
                tableViewModel.Table = new Table();
                tableViewModel.BuffetPrice = 4200;
                tableViewModel.BuffetName = "Dinner";
                tableViewModel.ReservedTableIds = reservedTables ?? new List<int>();

                // get available tables for today and do not have reservation for 5 pm to 7pm
                tableViewModel.Tables = context.Tables.FromSqlRaw($"GetAvailableTablesForToday {tableViewModel.BuffetName}").ToList();

                Console.WriteLine("ReservedTableIds count: " + (reservedTables?.Count ?? 0));
                TempData["SelectedDate"] = selectedDate;
                return View("SelectTable", tableViewModel);
            }

            TempData["ErrorMessage"] = $"Error getting available tables on {selectedDate}";
            return View("SelectTable", viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> CreateReservation(Reservation reservation)
        {
            try
            {
                // Create customer
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
            new SqlParameter("@TotalPrice", SqlDbType.Decimal) { Value = reservation.TotalPrice },
            new SqlParameter("@BuffetType", SqlDbType.VarChar) { Value = reservation.BuffetType },
            new SqlParameter("@SpecialRequest", SqlDbType.VarChar) { Value = string.IsNullOrEmpty(reservation.SpecialRequest) ? DBNull.Value : reservation.SpecialRequest },
            new SqlParameter("@TableId", SqlDbType.Int) { Value = reservation.TableId },
            new SqlParameter("@CustomerId", SqlDbType.Int) { Value = createdCustomerId }
                };

                var createReservationResult = context.Database.ExecuteSqlRaw($"Exec CreateReservation @ReservationDate, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId", reservationParameter);

                if (createReservationResult == 1)  // Checking if 1 row was affected
                {
                    // Send email after successful reservation
                    string subject = "Your Reservation Details";
                    string modifyUrl = Url.Action("EditReservation", "CustomerReservation", new { reservationId = reservation.Id }, Request.Scheme);
                    string cancelUrl = Url.Action("CancelReservation", "CustomerReservation", new { reservationId = reservation.Id }, Request.Scheme);

                    string body = $@"
                        <h2>Reservation Details</h2>
                        <p>Dear {reservation.Customer.FirstName} {reservation.Customer.LastName},</p>
                        <p>Your reservation has been confirmed. Present this email to the staff on the day of your reservation.</p>
                        <ul>
                            <li><strong>Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</li>
                            <li><strong>Buffet Type:</strong> {reservation.BuffetType}</li>
                            <li><strong>Table Number:</strong> {reservation.TableId}</li>
                            <li><strong>Total Price:</strong> ₱{reservation.TotalPrice}</li>
                        </ul>
                        <p>Modify or cancel your reservation using the buttons below:</p>
                        <a href='{modifyUrl}' style='padding: 10px 20px; background: #28a745; color: white; text-decoration: none;'>Modify Reservation</a>
                        <a href='{cancelUrl}' style='padding: 10px 20px; background: #dc3545; color: white; text-decoration: none; margin-left: 10px;'>Cancel Reservation</a>
                        <p>Best regards,<br>Restaurant Management Team</p>
            
                    ";

                    await _emailService.SendEmailAsync(reservation.Customer.Email, subject, body);

                    TempData["SuccessMessage"] = "Reservation created successfully. A confirmation email has been sent.";
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


        public IActionResult EditReservation(int reservationId)
        {

            try
            {
                var reservationToEdit = context.Reservations.FromSqlRaw($"GetReservationById {reservationId}").AsEnumerable().FirstOrDefault();

                return View("EditReservation", reservationToEdit);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("DisplayBuffets");
            }
        }

        public IActionResult CancelReservation(int reservationId)
        {
            var reservation = context.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("DisplayBuffets");
            }

            context.Reservations.Remove(reservation);
            context.SaveChanges();

            TempData["SuccessMessage"] = "Reservation canceled successfully.";
            return RedirectToAction("DisplayBuffets");
        }
    }


}
