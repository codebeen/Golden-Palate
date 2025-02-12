using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;
using RRS.Models.ViewModels;
using RRS.Services;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;

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

                TempData["BuffetDetails"] = "The meal period for this buffet is 8:00 AM to 10:00 AM. This buffet will only be available for that range of time.";


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

                TempData["BuffetDetails"] = "The meal period for this buffet is 11:30 AM to 2:00 PM. This buffet will only be available for that range of time.";

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

                TempData["BuffetDetails"] = "The meal period for this buffet is 5:00 PM to 8:00 PM. This buffet will only be available for that range of time.";

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

        public static string GenerateSecureReservationNumber()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[4];
                rng.GetBytes(buffer);
                int numberPart = BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF; // Ensure positive number
                return $"GP{numberPart % 900000 + 100000}"; // Keep it within 6-digit range
            }
        }

        // Display Reservation Form
        [HttpPost]
        public IActionResult DisplayReservationForm(int tableId, int tableNumber, decimal price, decimal buffetPrice, DateOnly date, string buffetType)
        {
            try
            {
                // Validate required inputs
                if (date == null)
                {
                    TempData["ErrorMessage"] = "Reservation date is required.";
                    return RedirectToAction("DisplayBuffets");
                }

                string reservationNumber = GenerateSecureReservationNumber();

                var table = context.Tables
                              .FromSqlRaw($"Exec GetTableById {tableId}")
                              .AsEnumerable()
                              .FirstOrDefault();



                Console.WriteLine(table.SeatingCapacity);
                // Create new reservation instance
                Reservation reservation = new Reservation
                {
                    ReservationNumber = reservationNumber,
                    Table = new Table
                    {
                        TableNumber = tableNumber,
                        Price = price,
                        SeatingCapacity = table.SeatingCapacity,
                    },
                    Customer = new Customer(),
                    TableId = tableId,
                    BuffetType = buffetType,
                    ReservationDate = date,
                    TotalPrice = price + buffetPrice
                };

                Console.WriteLine($"Table Reserved: {reservation.Table.TableNumber}, Date: {reservation.ReservationDate}");

                TempData["BuffetPrice"] = buffetPrice;
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

            // Add the current time to the selected date
            reservationDate = reservationDate.Date.Add(DateTime.Now.TimeOfDay);

            Console.WriteLine(reservationDate);  // Outputs the full DateTime (date + current time)
            Console.WriteLine(reservationDate.TimeOfDay);  // Outputs only the time part (current time)

            // Check if the selected date is in the past and prevent reservation
            if (reservationDate < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Reservation cannot be made for a date in the past.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
            }

            // Additional time checks for each buffet type (e.g., Breakfast, Lunch, Dinner)
            if (buffetType == "Breakfast" && reservationDate.Date == DateTime.Today && reservationDate.TimeOfDay >= TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30)))
            {
                TempData["ErrorMessage"] = "Breakfast reservations must be made before 7:30 AM today.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
            }

            if (buffetType == "Lunch" && reservationDate.Date == DateTime.Today && reservationDate.TimeOfDay >= TimeSpan.FromHours(11))
            {
                TempData["ErrorMessage"] = "Lunch reservations must be made before 11:00 AM today.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
            }

            if (buffetType == "Dinner" && reservationDate.Date == DateTime.Today && reservationDate.TimeOfDay >= TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(30)))
            {
                TempData["ErrorMessage"] = "Dinner reservations must be made before 4:30 PM today.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
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

                TempData["BuffetDetails"] = "The meal period for this buffet is 8:00 AM to 10:00 AM. This buffet will only be available for that range of time.";

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
                TempData["BuffetDetails"] = "The meal period for this buffet is 11:30 AM to 2:30 PM. This buffet will only be available for that range of time.";

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
                TempData["BuffetDetails"] = "The meal period for this buffet is 5:00 PM to 8:00 PM. This buffet will only be available for that range of time.";

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

                // Set up parameters
                var customerParameters = new SqlParameter[]
                {
                    new SqlParameter("@FirstName", SqlDbType.VarChar) { Value = customer.FirstName },
                    new SqlParameter("@LastName", SqlDbType.VarChar) { Value = customer.LastName },
                    new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { Value = customer.PhoneNumber },
                    new SqlParameter("@Email", SqlDbType.VarChar) { Value = customer.Email }
                };

                // Execute the stored procedure to create the customer and get the customer ID
                var createdCustomer = context.Customers.FromSqlRaw("EXEC CreateCustomer @FirstName, @LastName, @PhoneNumber, @Email", customerParameters)
                    .AsEnumerable()
                    .FirstOrDefault(); // Use FirstOrDefault (synchronous)

                var createdCustomerId = createdCustomer.Id;

                Console.WriteLine($"Created Customer ID: {createdCustomerId}");

                if (createdCustomerId == 0)
                {
                    TempData["ErrorMessage"] = "Unable to add customer.";
                    return View("DisplayBuffet");
                }

                // Now continue with the reservation creation
                var reservationParameters = new SqlParameter[]
                {
            new SqlParameter("@ReservationNumber", SqlDbType.VarChar) { Value = reservation.ReservationNumber },
            new SqlParameter("@ReservationDate", SqlDbType.Date) { Value = reservation.ReservationDate },
            new SqlParameter("@TotalPrice", SqlDbType.Decimal) { Value = reservation.TotalPrice },
            new SqlParameter("@BuffetType", SqlDbType.VarChar) { Value = reservation.BuffetType },
            new SqlParameter("@SpecialRequest", SqlDbType.VarChar) { Value = string.IsNullOrEmpty(reservation.SpecialRequest) ? DBNull.Value : reservation.SpecialRequest },
            new SqlParameter("@TableId", SqlDbType.Int) { Value = reservation.TableId },
            new SqlParameter("@CustomerId", SqlDbType.Int) { Value = createdCustomerId }
                };

                var createReservationResult = await context.Database.ExecuteSqlRawAsync(
                    "EXEC CreateReservation @ReservationNumber, @ReservationDate, @TotalPrice, @BuffetType, @SpecialRequest, @TableId, @CustomerId",
                    reservationParameters
                );

                if (createReservationResult == 1)  // Checking if 1 row was affected
                {
                    // Send email after successful reservation
                    string subject = "Your Reservation Details";
                    string modifyUrl = Url.Action("EditReservation", "CustomerReservation", new { reservationId = reservation.Id }, Request.Scheme);
                    string cancelUrl = Url.Action("CancelReservation", "CustomerReservation", new { reservationId = reservation.Id }, Request.Scheme);

                    // Get meal period and reservation time
                    (string mealPeriod, string reservationTime) = GetMealPeriodAndTime(reservation.BuffetType);

                    // Generate email body
                    string body = $@"
                <h2>Reservation Details</h2>
                <p>Dear {reservation.Customer.FirstName} {reservation.Customer.LastName},</p>
                <p>Your reservation has been confirmed. Present this email to the staff on the day of your reservation.</p>
                <p>Based on your selected buffet, your meal period is {mealPeriod}. Please arrive on or before {reservationTime}. Note that your reservation will be cancelled if you arrive 30 mins late.</p>
                <ul>
                    <li><strong>Reservation Number:</strong> {reservation.ReservationNumber}</li>
                    <li><strong>Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</li>
                    <li><strong>Time:</strong> {reservationTime}</li>
                    <li><strong>Buffet Type:</strong> {reservation.BuffetType}</li>
                    <li><strong>Table Number:</strong> {reservation.Table.TableNumber}</li>
                    <li><strong>Total Price:</strong> ₱{reservation.TotalPrice}</li>
                </ul>
                <p>Modify or cancel your reservation using the buttons below:</p>
                <a href='{modifyUrl}' style='padding: 10px 20px; background: #0096FF; color: white; text-decoration: none;'>Modify Reservation</a>
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



        private (string mealPeriod, string reservationTime) GetMealPeriodAndTime(string buffetType)
        {
            return buffetType switch
            {
                "Breakfast" => ("8:00 AM - 10:00 AM", "8:00 AM"),
                "Lunch" => ("11:30 AM - 2:30 PM", "11:30 AM"),
                "Dinner" => ("5:00 PM - 8:00 PM", "5:00 PM"),
                _ => ("Unknown", "Unknown") // Default case
            };
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
