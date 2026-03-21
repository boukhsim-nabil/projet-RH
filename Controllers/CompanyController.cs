using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using itgsgroup.Areas.Identity.Data;
using itgsgroup.Models.hrms;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.Data.SqlClient;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }


		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id)
        {
                ViewData["LocId"] = new SelectList(_context.locations, "Id", "name");
                ViewData["id"] = id;

            var companys = await _context.companies.Include(c => c.Loc).ToListAsync();
               var company = await _context.companies.FindAsync(id);      
                var companyview = new CompanyViewModel {
                    companyModels = companys
                };
            if (company != null)
            {
                companyview = new CompanyViewModel
                {
                    companyModels = companys,
                    name = company.name,
                    ntn = company.ntn,
                    stax = company.stax,
                    address = company.address,
                    LocId = company.LocId                    
                };
            }      
            return View(companyview);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,name,ntn,stax,address,LocId")] CompanyModel companyModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyModel);
                await _context.SaveChangesAsync();
                ViewData["LocId"] = new SelectList(_context.locations, "Id", "name", companyModel.LocId);
                return RedirectToAction(nameof(Index));
            }
            ViewData["LocId"] = new SelectList(_context.locations, "Id", "name", companyModel.LocId);
            return View(companyModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,ntn,stax,address,LocId")] CompanyModel companyModel)
        {
            if (id != companyModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyModelExists(companyModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LocId"] = new SelectList(_context.locations, "Id", "name", companyModel.LocId);
            return View("Index",companyModel);
        }

		[Authorize(Roles = "admin,HR")]
		[NonAction]
        public IActionResult ConfirmDelete(int id)
        {
            return View();
        }

		[Authorize(Roles = "admin,HR")]

		[HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ConfirmDelete")]
        public async Task<IActionResult> ConfirmDeletePost(int id)
        {
            ViewData["LocId"] = new SelectList(_context.locations, "Id", "name");

            var companys = await _context.companies.Include(c => c.Loc).ToListAsync();
            var companyview = new CompanyViewModel
            {

                companyModels = companys

            };
            try
            {
                var companyModel = await _context.companies.FindAsync(id);
                if (companyModel != null)
                {
                    _context.companies.Remove(companyModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle the case where the record with the given ID doesn't exist
                    return NotFound();
                }
            }
            catch (DbUpdateException ex)
            {
                if (IsForeignKeyViolation(ex))
                {
                    // Handle the foreign key violation error
                    // You can add a custom error message or take appropriate action
                    ModelState.AddModelError(string.Empty, "Cannot delete the record due to existing references.");
                }
                else
                {
                    // Handle other database update exceptions or rethrow the exception for further handling
                    throw;
                }
            }
           

            // If an exception occurs, you can return to a relevant view
            return View("Index",companyview);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpGet]
        [ActionName("ConfirmDelete")]
        public IActionResult ConfirmDeleteGet(int id)
        {
            // This action handles the GET request to display a confirmation view
            return View();
        }
        // Helper method to check for foreign key violation
        private bool IsForeignKeyViolation(DbUpdateException ex)
        {
            var sqlException = ex.InnerException as SqlException;
            return sqlException != null && sqlException.Number == 547;
        }

        private bool CompanyModelExists(int id)
        {
          return (_context.companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
