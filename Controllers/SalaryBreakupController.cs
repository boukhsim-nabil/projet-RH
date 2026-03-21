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
	public class SalaryBreakupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalaryBreakupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            var salaryBreakups = await _context.salaryBreakup.Include(s => s.company).ToListAsync();
            var salaryBreakup = await _context.salaryBreakup.FindAsync(id);
            var salaryBreakupview = new SalaryBreakupViewModel
            {
                salaryBreakups = salaryBreakups
            };
            if (salaryBreakup != null)
            {
                salaryBreakupview = new SalaryBreakupViewModel
                {
                    salaryBreakups = salaryBreakups,
                    name = salaryBreakup.name,
                    percent = salaryBreakup.percent,
                    companyId = salaryBreakup.companyId
                };
            }
            return View(salaryBreakupview);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,name,percent,companyId")] SalaryBreakupModel salaryBreakupModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(salaryBreakupModel);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }         
          
            return View(salaryBreakupModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,percent,companyId")] SalaryBreakupModel salaryBreakupModel)
        {
            if (id != salaryBreakupModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salaryBreakupModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalaryBreakupModelExists(salaryBreakupModel.Id))
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
            return View("Index", salaryBreakupModel);
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

            var salaryBreakups = await _context.salaryBreakup.Include(c => c.company).ToListAsync();
            var salaryBreakupview = new SalaryBreakupViewModel
            {

                salaryBreakups = salaryBreakups

            };
            try
            {
                if (_context.salaryBreakup == null)
            {
                return Problem("Entity set 'ApplicationDbContext.salaryBreakup'  is null.");
            }
            var salaryBreakupModel = await _context.salaryBreakup.FindAsync(id);
            if (salaryBreakupModel != null)
            {
                _context.salaryBreakup.Remove(salaryBreakupModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (IsForeignKeyViolation(ex))
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete the record due to existing references.");
                }
                else
                {
                    throw;
                }
            }
            return View("Index", salaryBreakupview);
        }
		[Authorize(Roles = "admin,HR")]
		[HttpGet]
        [ActionName("ConfirmDelete")]
        public IActionResult ConfirmDeleteGet(int id)
        {
            return View();
        }
        private bool IsForeignKeyViolation(DbUpdateException ex)
        {
            var sqlException = ex.InnerException as SqlException;
            return sqlException != null && sqlException.Number == 547;
        }

        private bool SalaryBreakupModelExists(int id)
        {
          return (_context.salaryBreakup?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
