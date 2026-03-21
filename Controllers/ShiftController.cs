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
	public class ShiftController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShiftController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            var shifts = await _context.shift.Include(c => c.company).ToListAsync();
            var shift = await _context.shift.FindAsync(id);
            var shiftview = new ShiftViewModel
            {
                shifts = shifts,
                from = null,
                to = null
            };
            if (shift != null)
            {
                shiftview = new ShiftViewModel
                {
                    shifts = shifts,
                    from = shift.from,
                    to = shift.to,
                    companyId = shift.companyId
                };
            }
            return View(shiftview);
        }
		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,from,to,companyId")] ShiftModel shiftModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                _context.Add(shiftModel);
                await _context.SaveChangesAsync();
               
                return RedirectToAction(nameof(Index));
            }
           
            return View(shiftModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,from,to,companyId")] ShiftModel shiftModel)
        {
            if (id != shiftModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shiftModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftModelExists(shiftModel.Id))
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
            return View("Index",shiftModel);
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

            var shifts = await _context.shift.Include(c => c.company).ToListAsync();
            var shiftview = new ShiftViewModel
            {

                shifts = shifts,
                from = null,
                to = null

            };
            try
            {
                if (_context.shift == null)
            {
                return Problem("Entity set 'ApplicationDbContext.shift'  is null.");
            }
            var shiftModel = await _context.shift.FindAsync(id);
            if (shiftModel != null)
            {
                _context.shift.Remove(shiftModel);
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
            return View("Index", shiftview);
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

        private bool ShiftModelExists(int id)
        {
          return (_context.shift?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
