using Microsoft.AspNetCore.Mvc;
using RRS.Data;
using RRS.Models;

namespace RRS.Controllers
{
    public class TableController : Controller
    {
        private readonly ApplicationDbContext context;

        public TableController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<Table> GetTables()
        {
            List<Table> tables = context.Tables.Where(t => !t.IsDeleted).ToList();
            return tables;
        }

        public IActionResult Index()
        {
            var tables = this.GetTables();

            return View(tables);
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
                    // Set timestamps
                    table.CreatedAt = DateTime.Now;
                    table.UpdatedAt = DateTime.Now;

                    // Add the new table to the database
                    context.Tables.Add(table);
                    context.SaveChanges();

                    TempData["SuccessMessage"] = "Table added successfully.";
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

                    // Return the view with the same model to show validation errors
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;

                // Return the view with the same model to show the error
                return RedirectToAction("Index");
            }
        }


        public IActionResult Edit(int id)
        {
            try
            {
                Table tableToEdit = context.Tables.Where(t => t.Id == id).FirstOrDefault();

                return PartialView("EditTableModal", tableToEdit);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Table table)
        {
            if (ModelState.IsValid)
            {
                Table existingTable = context.Tables.FirstOrDefault(t => t.Id == table.Id);

                if (existingTable != null)
                {
                    if (table.TableNumber == existingTable.TableNumber && table.SeatingCapacity == existingTable.SeatingCapacity
                        && table.TableLocation == existingTable.TableLocation && table.Description == existingTable.Description)
                    {
                        TempData["InformationMessage"] = "No changes have been made";
                        return RedirectToAction("Index");
                    }

                    existingTable.TableNumber = table.TableNumber;
                    existingTable.SeatingCapacity = table.SeatingCapacity;
                    existingTable.TableLocation = table.TableLocation;
                    existingTable.Price = table.Price;
                    existingTable.Description = table.Description;
                    existingTable.UpdatedAt = DateTime.Now;

                    context.SaveChanges();

                    TempData["SuccessMessage"] = "Table details updated successfuly.";
                    return RedirectToAction("Index");
                }
            }

            TempData["ErrorMessage"] = "Invalid table data or not found.";
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            try
            {
                Table tableToDelete = context.Tables.Where(t => t.Id == id).FirstOrDefault();

                return PartialView("DeleteTableModal", tableToDelete);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Table not found.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Delete(Table table)
        {
            try
            {
                Table existingTable = context.Tables.FirstOrDefault(t => t.Id == table.Id);

                if (existingTable != null)
                {
                    existingTable.IsDeleted = true;
                    existingTable.UpdatedAt = DateTime.Now;

                    context.SaveChanges();

                    TempData["SuccessMessage"] = "Table successfully deleted.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Table not found";
                return RedirectToAction("Index");
            }
        }

    }
}