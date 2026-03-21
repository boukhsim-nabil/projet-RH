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
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class FascalYearController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FascalYearController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            List<Tuple<string, int>> companys = _context.companies
           .Select(c => Tuple.Create(c.name, c.Id))
           .ToList();
            ViewBag.Companys = companys;
            ViewData["id"] = id;
            var fascalyears = await _context.fascalYears.Include(c => c.company).ToListAsync();
            var fascalyear = await _context.fascalYears.FindAsync(id);
            var fascalyearview = new fascalYearViewModel
            {
                fascalYears = fascalyears,
                from = null,
                to = null
            };
            if (fascalyear != null)
            {
                fascalyearview = new fascalYearViewModel
                {
                    fascalYears = fascalyears,
                    from = fascalyear.from,
                    to = fascalyear.to,
                    companyId = fascalyear.companyId
                };
            }
            return View(fascalyearview);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,from,to,companyId")] FascalYearModel fascalYearModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(fascalYearModel);
                await _context.SaveChangesAsync();
              
                return RedirectToAction(nameof(Index));
            }
            return View(fascalYearModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,from,to,companyId")] FascalYearModel fascalYearModel)
        {
            if (id != fascalYearModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fascalYearModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FascalYearModelExists(fascalYearModel.Id))
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
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            return View("Index", fascalYearModel);
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
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;

            var fascalyears = await _context.fascalYears.Include(c => c.company).ToListAsync();
            var fascalyearview = new fascalYearViewModel
            {

                fascalYears = fascalyears

            };
            try
            {
                if (_context.fascalYears == null)
            {
                return Problem("Entity set 'ApplicationDbContext.fascalYears'  is null.");
            }
            var fascalYearModel = await _context.fascalYears.FindAsync(id);
            if (fascalYearModel != null)
            {
                _context.fascalYears.Remove(fascalYearModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            return View("Index", fascalyearview);
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



        private bool FascalYearModelExists(int id)
        {
          return (_context.fascalYears?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
