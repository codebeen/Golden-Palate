using Microsoft.AspNetCore.DataProtection;
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
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public CustomerReservationController(ApplicationDbContext context, EmailService emailService, IDataProtectionProvider dataProtectionProvider)
        {
            this.context = context;
            _emailService = emailService;
            _dataProtectionProvider = dataProtectionProvider;
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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                              .FromSqlRaw("Exec GetTableById @p0", tableId)
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
                TempData["ErrorMessage"] = "Breakfast reservations for today must be made before 7:30 AM.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
            }

            if (buffetType == "Lunch" && reservationDate.Date == DateTime.Today && reservationDate.TimeOfDay >= TimeSpan.FromHours(11))
            {
                TempData["ErrorMessage"] = "Lunch reservations today must be made before 11:00 AM.";
                return RedirectToAction("DisplayBuffets"); // Redirect to an error page or show an error message
            }

            if (buffetType == "Dinner" && reservationDate.Date == DateTime.Today && reservationDate.TimeOfDay >= TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(30)))
            {
                TempData["ErrorMessage"] = "Dinner reservations today must be made before 4:30 PM.";
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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                tableViewModel.Tables = context.Tables.FromSqlRaw("GetAvailableTablesForToday @p0", tableViewModel.BuffetName).ToList();

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
                    var createdReservation = context.Reservations
                        .FromSqlRaw("EXEC GetReservationByNumber @p0", reservation.ReservationNumber)
                        .AsEnumerable()
                        .FirstOrDefault();

                    var protector = _dataProtectionProvider.CreateProtector("ReservationIdProtector");
                    string encryptedId = protector.Protect(createdReservation.Id.ToString());

                    // Send email after successful reservation
                    string subject = "Your Reservation Details";
                    string modifyUrl = Url.Action("EditReservation", "CustomerReservation", new { reservationId = encryptedId }, Request.Scheme);
                    string cancelUrl = Url.Action("CancelReservation", "CustomerReservation", new { reservationId = encryptedId }, Request.Scheme);

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
                <p><strong>Note: </strong> Only the reservation date and your personal details can be modified. If you wish to change your table or buffet selection, please cancel this reservation and create a new one.</p>
                <br>
                <p>Modify or cancel your reservation using the buttons below:</p>
                <a href='{modifyUrl}' style='padding: 10px 20px; background: #0096FF; color: white; text-decoration: none;'>Modify Reservation</a>
                <a href='{cancelUrl}' style='padding: 10px 20px; background: #dc3545; color: white; text-decoration: none; margin-left: 10px;'>Cancel Reservation</a>
                <p>Best regards,<br>Golden Palate Team</p>
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

        [HttpGet]
        public IActionResult EditReservation(string reservationId)
        {
            try
            {
                // decrypt the id
                var protector = _dataProtectionProvider.CreateProtector("ReservationIdProtector");
                int id = int.Parse(protector.Unprotect(reservationId));

                var reservationToEdit = context.ReservationDetailsDto.FromSqlRaw("GetReservationById @p0", id).AsEnumerable().FirstOrDefault();

                if (reservationToEdit == null)
                {
                    TempData["ErrorMessage"] = "Reservation not found.";
                    return RedirectToAction("DisplayBuffets");
                }

                if (reservationToEdit.Status.ToLower() == "cancelled")
                {
                    TempData["ErrorMessage"] = "This reservation is cancelled.";
                    return RedirectToAction("DisplayBuffets");
                }

                var reservedDates = context.Reservations
                                  .FromSqlRaw("EXEC GetReservedDatesForTableId @p0, @p1", reservationToEdit.TableId, reservationToEdit.BuffetType)
                                  .AsEnumerable()
                                  .Select(r => r.ReservationDate)
                                  .ToList();

                if (reservedDates == null)
                {
                    TempData["ErrorMessage"] = "Reservation dates not found.";
                    return RedirectToAction("DisplayBuffets");
                }

                EditReservationViewModel reservationToEditViewModel = new EditReservationViewModel
                {
                    ReservationDetails = reservationToEdit,
                    ReservedDates = reservedDates ?? new List<DateOnly>()
                };

                Console.WriteLine("Reserved dates:");
                foreach (var date in reservationToEditViewModel.ReservedDates)
                {
                    Console.WriteLine(date.ToString("yyyy-MM-dd")); // Or any format you prefer
                }
                Console.WriteLine(reservationToEditViewModel.ReservationDetails.ReservationNumber);

                return View("EditReservation", reservationToEditViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("DisplayBuffets");
            }
        }

        [HttpPost]
        public IActionResult UpdateReservation(EditReservationViewModel reservation)
        {
            try
            {
                Console.WriteLine(reservation.ReservationDetails.FirstName);
                var customerToEdit = context.Customers.FromSqlRaw("Exec GetCustomerById @p0", reservation.ReservationDetails.CustomerId).AsEnumerable().FirstOrDefault();

                if (customerToEdit == null)
                {
                    TempData["ErrorMessage"] = "Customer not found.";
                    return RedirectToAction("DisplayBuffets");
                }

                var customerParameters = new SqlParameter[]
                {
                    new SqlParameter()
                    {
                        ParameterName = "@Id",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Value = reservation.ReservationDetails.CustomerId
                    },
                    new SqlParameter()
                    {
                        ParameterName = "@FirstName",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Value = reservation.ReservationDetails.FirstName
                    },
                    new SqlParameter()
                    {
                        ParameterName = "@LastName",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Value = reservation.ReservationDetails.LastName
                    },
                    new SqlParameter()
                    {
                        ParameterName = "@PhoneNumber",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Value = reservation.ReservationDetails.PhoneNumber
                    },
                    new SqlParameter()
                    {
                        ParameterName = "@Email",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Value = reservation.ReservationDetails.Email
                    }
                };

                // update customer details
                var updateCustomerResult = context.Database.ExecuteSqlRaw("EXEC UpdateCustomer @Id, @FirstName, @LastName, @PhoneNumber, @Email", customerParameters);

                if (updateCustomerResult == 0)
                {
                    TempData["ErrorMessage"] = "Failed to update customer";
                    return View("EditReservation", reservation);
                }

                var reservationToEdit = context.ReservationDetailsDto.FromSqlRaw("Exec GetReservationById @p0", reservation.ReservationDetails.Id).AsEnumerable().FirstOrDefault();

                if (reservationToEdit == null)
                {
                    TempData["ErrorMessage"] = "Reservation not found.";
                    return RedirectToAction("DisplayBuffets");
                }

                var reservationParameters = new SqlParameter[]
                {
                    new SqlParameter()
                    {
                        ParameterName = "@Id",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Value = reservation.ReservationDetails.Id
                    },
                    new SqlParameter()
                    {
                        ParameterName = "@ReservationDate",
                        SqlDbType = System.Data.SqlDbType.Date,
                        Value = reservation.ReservationDetails.ReservationDate
                    }
                };

                // update customer details
                var updateReservationResult = context.Database.ExecuteSqlRaw("EXEC UpdateReservation @Id, @ReservationDate", reservationParameters);

                if (updateCustomerResult == 0)
                {
                    TempData["ErrorMessage"] = "Failed to update reservation details";
                    return View("EditReservation", reservation);
                }

                var protector = _dataProtectionProvider.CreateProtector("ReservationIdProtector");
                string encryptedId = protector.Protect(reservation.ReservationDetails.Id.ToString());

                // Send email after successful reservation
                string subject = "Your Edited Reservation Details";
                string modifyUrl = Url.Action("EditReservation", "CustomerReservation", new { reservationId = encryptedId }, Request.Scheme);
                string cancelUrl = Url.Action("CancelReservation", "CustomerReservation", new { reservationId = encryptedId }, Request.Scheme);

                // Get meal period and reservation time
                (string mealPeriod, string reservationTime) = GetMealPeriodAndTime(reservation.ReservationDetails.BuffetType);

                // Generate email body
                string body = $@"
                <h2>Reservation Details</h2>
                <p>Dear {reservation.ReservationDetails.FirstName} {reservation.ReservationDetails.LastName},</p>
                <p>Your reservation has been updated. Present this email to the staff on the day of your reservation.</p>
                <p>Based on your selected buffet, your meal period is {mealPeriod}. Please arrive on or before {reservationTime}. Note that your reservation will be cancelled if you arrive 30 mins late.</p>
                <ul>
                    <li><strong>Reservation Number:</strong> {reservation.ReservationDetails.ReservationNumber}</li>
                    <li><strong>Date:</strong> {reservation.ReservationDetails.ReservationDate:MMMM dd, yyyy}</li>
                    <li><strong>Time:</strong> {reservationTime}</li>
                    <li><strong>Buffet Type:</strong> {reservation.ReservationDetails.BuffetType}</li>
                    <li><strong>Table Number:</strong> {reservation.ReservationDetails.TableNumber}</li>
                    <li><strong>Total Price:</strong> ₱{reservation.ReservationDetails.TotalPrice}</li>
                </ul>
                <p><strong>Note: </strong> Only the reservation date and your personal details can be modified. If you wish to change your table or buffet selection, please cancel this reservation and create a new one.</p>
                <p>Modify or cancel your reservation using the buttons below:</p>
                <a href='{modifyUrl}' style='padding: 10px 20px; background: #0096FF; color: white; text-decoration: none;'>Modify Reservation</a>
                <a href='{cancelUrl}' style='padding: 10px 20px; background: #dc3545; color: white; text-decoration: none; margin-left: 10px;'>Cancel Reservation</a>
                <p>Best regards,<br>Restaurant Management Team</p>
                ";

                _emailService.SendEmailAsync(reservation.ReservationDetails.Email, subject, body);

                TempData["SuccessMessage"] = "Reservation updated successfully. Your reservation details has been sent to your email.";
                return RedirectToAction("DisplayBuffets");

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("EditReservation", reservation);
            }
        }


        [HttpGet]
        public IActionResult CancelReservation(string reservationId)
        {
            // decrypt the id
            var protector = _dataProtectionProvider.CreateProtector("ReservationIdProtector");
            int id = int.Parse(protector.Unprotect(reservationId));

            var reservation = context.Reservations.FromSqlRaw($"Exec GetReservationById @p0", id).AsEnumerable().FirstOrDefault();
            
            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Reservation not found.";
                return RedirectToAction("DisplayBuffets");
            }


            TempData["DeleteReservation"] = $"Are you sure you want to delete your reservation with Reservation Number: {reservation.ReservationNumber}? This action cannot be undone.";
            TempData["ReservationNumber"] = reservation.ReservationNumber;
            return RedirectToAction("DisplayBuffets");
        }

        [HttpPost]
        public IActionResult CancelReservationInDatabase(string reservationNumber)
        {
            var cancelReservationResult = context.Database.ExecuteSqlRaw($"Exec CancelReservation @p0", reservationNumber);

            if (cancelReservationResult == 1)
            {
                TempData["SuccessMessage"] = "Successfully cancelled your reservation.";
                return RedirectToAction("DisplayBuffets");
            }

            TempData["ErrorMessage"] = "Unable to cancel your reservation.";
            return RedirectToAction("DisplayBuffets");
        }
    }


}
