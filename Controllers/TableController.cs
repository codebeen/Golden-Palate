using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RRS.Data;
using RRS.Models;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace RRS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TableController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<TableController> logger;
        private readonly IWebHostEnvironment environment;

        public TableController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<TableController> logger)
        {
            this.context = context;
            this.environment = environment;
            this.logger = logger;
        }


        public IActionResult Index()
        {
            var tables = context.Tables.FromSqlRaw("GetAllTables").ToList();

            return View(tables);
        }

        // View Single Record
        public IActionResult GetTableDetails(int? id)
        {
            var table = context.Tables.FromSqlRaw("GetTableById").AsEnumerable().FirstOrDefault();

            return PartialView("ViewTableDetails", table);
        }


        //public IActionResult DisplayTables()
        //{
        //    TableViewModel tableViewModel = new TableViewModel();

        //    tableViewModel.Tables = this.GetTables();
        //    tableViewModel.Reservation = new Reservation();
        //    tableViewModel.Table = new Table();

        //    return View("DisplayTables", tableViewModel);
        //}

        public IActionResult Create()
        {
            Table table = new Table();

            return PartialView("AddTableModal", table);
        }

        [HttpPost]
        public IActionResult Create(Table table)
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
                if (ModelState.IsValid)
                {
                    string? imagePath = null;

                    // Check if an image file is uploaded
                    if (table.TableImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(environment.WebRootPath, "tables");

                        // Ensure the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate a unique filename
                        string newFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(table.TableImageFile.FileName)}";
                        string imageFullPath = Path.Combine(uploadsFolder, newFileName);

                        // Save the uploaded image
                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            table.TableImageFile.CopyTo(stream);
                        }

                        // Save relative file path to the database
                        imagePath = "/tables/" + newFileName;
                    }

                    // Use SQL parameters
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TableNumber", System.Data.SqlDbType.Int) { Value = table.TableNumber },
                        new SqlParameter("@Description", System.Data.SqlDbType.VarChar) { Value = table.Description ?? (object)DBNull.Value },
                        new SqlParameter("@SeatingCapacity", System.Data.SqlDbType.Int) { Value = table.SeatingCapacity },
                        new SqlParameter("@TableLocation", System.Data.SqlDbType.VarChar) { Value = table.TableLocation },
                        new SqlParameter("@Price", System.Data.SqlDbType.Decimal) { Value = table.Price },
                        new SqlParameter("@TableImagePath", System.Data.SqlDbType.VarChar) { Value = imagePath ?? (object)DBNull.Value }
                    };

                    int createTableResult = context.Database.ExecuteSqlRaw("Exec CreateTable @TableNumber, @Description, @SeatingCapacity, @TableLocation, @Price, @TableImagePath", parameters);

                    if (createTableResult == 1)
                    {
                        var logParams = new SqlParameter[]
                        {
                            new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Created a table" },
                            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                        };
                        context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                        TempData["SuccessMessage"] = "Table added successfully.";
                        return RedirectToAction("Index");
                    }

                    var errorlogParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to create table" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errorlogParams);

                    TempData["ErrorMessage"] = "Failed to add the table.";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Capture validation errors
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    Console.WriteLine(errors);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                TempData["ErrorMessage"] = "Failed to add the table.";
                return RedirectToAction("Index");
            }
        }


        public IActionResult Edit(int id)
        {
            try
            {
                var tableToEdit = context.Tables.FromSqlRaw("GetTableById @p0", id).AsEnumerable().FirstOrDefault();

                return PartialView("EditTableModal", tableToEdit);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Edit(Table table)
        {
            // Retrieve UserId securely from authentication claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                logger.LogWarning("Invalid or missing UserId in claims.");
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var existingTable = context.Tables.FromSqlRaw("GetTableById @p0", table.Id).AsEnumerable().FirstOrDefault();

                if (existingTable != null)
                {
                    // Check if there are any changes in the table details
                    if (table.TableNumber == existingTable.TableNumber && table.SeatingCapacity == existingTable.SeatingCapacity
                        && table.TableLocation == existingTable.TableLocation && table.Description == existingTable.Description
                        && table.Price == existingTable.Price && table.TableImagePath == existingTable.TableImagePath)
                    {
                        TempData["InformationMessage"] = "No changes have been made";
                        return RedirectToAction("Index");
                    }

                    // Update table properties
                    existingTable.TableNumber = table.TableNumber;
                    existingTable.SeatingCapacity = table.SeatingCapacity;
                    existingTable.TableLocation = table.TableLocation;
                    existingTable.Price = table.Price;
                    existingTable.Description = table.Description;
                    existingTable.Status = table.Status;

                    // Handle image upload if a new image is uploaded
                    if (table.TableImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(environment.WebRootPath, "tables");

                        // Ensure the folder exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate a unique file name for the image
                        string newFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(table.TableImageFile.FileName)}";
                        string imageFullPath = Path.Combine(uploadsFolder, newFileName);

                        // Save the uploaded image
                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            table.TableImageFile.CopyTo(stream);
                        }

                        // Update the TableImagePath
                        existingTable.TableImagePath = "/tables/" + newFileName;
                    }
                    else
                    {
                        // If no new image is uploaded, keep the old image path
                        existingTable.TableImagePath = existingTable.TableImagePath;
                    }

                    // Prepare parameters for stored procedure execution
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter() { ParameterName = "@Id", SqlDbType = System.Data.SqlDbType.Int, Value = existingTable.Id },
                        new SqlParameter() { ParameterName = "@TableNumber", SqlDbType = System.Data.SqlDbType.Int, Value = existingTable.TableNumber },
                        new SqlParameter() { ParameterName = "@Description", SqlDbType = System.Data.SqlDbType.VarChar, Value = existingTable.Description },
                        new SqlParameter() { ParameterName = "@SeatingCapacity", SqlDbType = System.Data.SqlDbType.Int, Value = existingTable.SeatingCapacity },
                        new SqlParameter() { ParameterName = "@TableLocation", SqlDbType = System.Data.SqlDbType.VarChar, Value = existingTable.TableLocation },
                        new SqlParameter() { ParameterName = "@Price", SqlDbType = System.Data.SqlDbType.Decimal, Value = existingTable.Price },
                        new SqlParameter() { ParameterName = "@TableImagePath", SqlDbType = System.Data.SqlDbType.VarChar, Value = existingTable.TableImagePath },
                        new SqlParameter() { ParameterName = "@Status", SqlDbType = System.Data.SqlDbType.VarChar, Value = existingTable.Status }
                    };

                    // Execute stored procedure to update table
                    var updateTableResult = context.Database.ExecuteSqlRaw($"Exec UpdateTable @Id, @TableNumber, @Description, @SeatingCapacity, @TableLocation, @Price, @TableImagePath, @Status", parameters);

                    if (updateTableResult == 1)
                    {
                        var logParams = new SqlParameter[]
                        {
                            new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Updated a table" },
                            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                        };
                        context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                        TempData["SuccessMessage"] = "Table details updated successfully.";
                        return RedirectToAction("Index");
                    }

                    var errologParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to update a table" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errologParams);

                    TempData["ErrorMessage"] = "Failed to update table.";
                    return RedirectToAction("Index");
                }

                // Capture validation errors if no table was found
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                Console.WriteLine(errors);

                // Return the view with the same model to show validation errors
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Invalid table data or not found.";
            return RedirectToAction("Index");
        }



        public IActionResult Delete(int id)
        {
            try
            {
                var tableToDelete = context.Tables.FromSqlRaw("GetTableById @p0", id).AsEnumerable().FirstOrDefault();

                return PartialView("DeleteTableModal", tableToDelete);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Delete(Table table)
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
                var existingTable = context.Tables.FromSqlRaw("GetTableById @p0", table.Id).AsEnumerable().FirstOrDefault();

                if (existingTable != null)
                {
                    // Create the parameter array with proper SqlParameter setup
                    var parameter = new SqlParameter[]
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@Id",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = table.Id
                        },
                    };

                    // Execute the stored procedure with the parameter array
                    var deleteTableResult = context.Database.ExecuteSqlRaw("EXEC DeleteTable @Id", parameter);

                    var logParams = new SqlParameter[]
                    {
                        new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Deleted a table" },
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
                    };
                    context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

                    TempData["SuccessMessage"] = "Table successfully deleted.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Table not found";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var errologParams = new SqlParameter[]
                {
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Failed to delete a table" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Failed" }
                };
                context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", errologParams);

                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occurred while deleting the table.";
                return RedirectToAction("Index");
            }
        }


        public ActionResult Export()
        {
            var tables = context.Tables.FromSqlRaw("GetAllTables").ToList();

            var csvFileName = $"tables_{DateTime.Now:yyyy-MM-dd}.csv";
            var csvContent = new StringBuilder();

            // Add CSV headers
            csvContent.AppendLine("Table Number,Description,Seating Capacity,Table Location,Price,Status");

            foreach (var table in tables)
            {
                csvContent.AppendLine($"{table.TableNumber},{table.Description},{table.SeatingCapacity},{table.TableLocation},{table.Price},{table.Status}");
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
                    new SqlParameter("@Activity", SqlDbType.VarChar) { Value = "Export all tables into csv file" },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Success" }
            };
            context.Database.ExecuteSqlRaw("EXEC InsertAuditLog @Activity, @UserId, @Status", logParams);

            return File(stream, "text/csv", csvFileName);
        }

    }
}