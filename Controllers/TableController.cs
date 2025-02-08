using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RRS.Data;
using RRS.Models;

namespace RRS.Controllers
{
    public class TableController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public TableController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
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
                        TempData["SuccessMessage"] = "Table added successfully.";
                        return RedirectToAction("Index");
                    }

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

                    TempData["ErrorMessage"] = "Validation errors: " + string.Join(", ", errors);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        public IActionResult Edit(int id)
        {
            try
            {
                var tableToEdit = context.Tables.FromSqlRaw($"GetTableById {id}").AsEnumerable().FirstOrDefault();

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
            if (ModelState.IsValid)
            {
                var existingTable = context.Tables.FromSqlRaw($"GetTableById {table.Id}").AsEnumerable().FirstOrDefault();

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
                        TempData["SuccessMessage"] = "Table details updated successfully.";
                        return RedirectToAction("Index");
                    }

                    TempData["ErrorMessage"] = "Failed to update table.";
                    return RedirectToAction("Index");
                }

                // Capture validation errors if no table was found
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = "Validation errors: " + string.Join(", ", errors);

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
                var tableToDelete = context.Tables.FromSqlRaw($"GetTableById {id}").AsEnumerable().FirstOrDefault();

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
            try
            {
                var existingTable = context.Tables.FromSqlRaw($"GetTableById {table.Id}").AsEnumerable().FirstOrDefault();

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

                    TempData["SuccessMessage"] = "Table successfully deleted.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Table not found";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occurred while deleting the table.";
                return RedirectToAction("Index");
            }
        }

    }
}