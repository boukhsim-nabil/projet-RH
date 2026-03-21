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
using Microsoft.AspNetCore.Http.HttpResults;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class CompanyHolidayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyHolidayController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id)
        {
            List<Tuple<string, int>> companys = _context.companies
           .Select(c => Tuple.Create(c.name, c.Id))
           .ToList();
            ViewBag.Companys = companys;
            var user = await _userManager.GetUserAsync(User);
            ViewData["id"] = id;
            var companyholidays = await _context.companyHolidays.Include(c => c.company)
                .OrderByDescending(c => c.date).ToListAsync();
            var companyholiday = await _context.companyHolidays.FindAsync(id);
            var companyholidayvew = new CompanyHolidayViewModel
            {
                companyHolidays = companyholidays,
                date =null
            };
            if (companyholiday != null)
            {
                companyholidayvew = new CompanyHolidayViewModel
                {
                    companyHolidays = companyholidays,
                    date = companyholiday.date,
                    name = companyholiday.name,
                    companyId = companyholiday.companyId
                   
                };
            }
            return View(companyholidayvew);
        }





		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,name,date,companyId")] CompanyHolidayModel companyholidayModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(companyholidayModel);
                await _context.SaveChangesAsync();               
                return RedirectToAction(nameof(Index));
            }
            
            return View(companyholidayModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,date,companyId")] CompanyHolidayModel companyholidayModel)
        {
            if (id != companyholidayModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyholidayModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!companyholidayModelExists(companyholidayModel.Id))
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
            return View("Index", companyholidayModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.companyHolidays == null)
            {
                return Problem("Entity set 'ApplicationDbContext.companyHolidays'  is null.");
            }
            var companyholidayModel = await _context.companyHolidays.FindAsync(id);
            if (companyholidayModel != null)
            {
                _context.companyHolidays.Remove(companyholidayModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool companyholidayModelExists(int id)
        {
          return (_context.companyHolidays?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
