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
	public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["id"] = id;
            var departments = await _context.departments.Include(c => c.company).ToListAsync();
            var department = await _context.departments.FindAsync(id);
            var departmentvew = new departmentViewModel
            {
                departments = departments
            };
            if (department != null)
            {
                departmentvew = new departmentViewModel
                {
                    departments = departments,
                    name = department.name,
                    companyId = department.companyId
                   
                };
            }
            return View(departmentvew);
        }





		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,name,companyId")] DepartmentModel departmentModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(departmentModel);
                await _context.SaveChangesAsync();
                ViewData["companyId"] = user.companyId;
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["companyId"] = user.companyId;
            return View(departmentModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,companyId")] DepartmentModel departmentModel)
        {
            if (id != departmentModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(departmentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentModelExists(departmentModel.Id))
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
            return View("Index",departmentModel);
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

            var departments = await _context.departments.Include(c => c.company).ToListAsync();
            var departmentview = new departmentViewModel
            {

                departments = departments

            };
            try { 
            if (_context.departments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.departments'  is null.");
            }
            var departmentModel = await _context.departments.FindAsync(id);
            if (departmentModel != null)
            {
                _context.departments.Remove(departmentModel);
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
            return View("Index", departmentview);
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


        private bool DepartmentModelExists(int id)
        {
          return (_context.departments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
