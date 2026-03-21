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
	public class GazettedHolidayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GazettedHolidayController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            var gazettedholidays = await _context.gazettedHolidays.Include(c => c.company)
                .OrderByDescending(c => c.date).ToListAsync();
            var gazettedholiday = await _context.gazettedHolidays.FindAsync(id);
            var gazettedholidayvew = new GazettedHolidayViewModel
            {
                gazettedHolidays = gazettedholidays,
                date =null
            };
            if (gazettedholiday != null)
            {
                gazettedholidayvew = new GazettedHolidayViewModel
                {
                    gazettedHolidays = gazettedholidays,
                    date = gazettedholiday.date,
                    name = gazettedholiday.name,
                    companyId = gazettedholiday.companyId
                   
                };
            }
            return View(gazettedholidayvew);
        }





		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,name,date,companyId")] GazettedHolidayModel gazettedholidayModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(gazettedholidayModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           
            return View(gazettedholidayModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,date,companyId")] GazettedHolidayModel gazettedholidayModel)
        {
            if (id != gazettedholidayModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gazettedholidayModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!gazettedholidayModelExists(gazettedholidayModel.Id))
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
            return View("Index",gazettedholidayModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.gazettedHolidays == null)
            {
                return Problem("Entity set 'ApplicationDbContext.gazettedHolidays'  is null.");
            }
            var gazettedholidayModel = await _context.gazettedHolidays.FindAsync(id);
            if (gazettedholidayModel != null)
            {
                _context.gazettedHolidays.Remove(gazettedholidayModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool gazettedholidayModelExists(int id)
        {
          return (_context.gazettedHolidays?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
