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
	public class SlabsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SlabsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id,string? Modelaction)
        {
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from.ToString("dd/MM/yyyy")} - {fy.to.ToString("dd/MM/yyyy")}",
                Value = fy.Id.ToString()
            }).ToList();
            var user = await _userManager.GetUserAsync(User);
            List<Tuple<string, int>> companys = _context.companies
          .Select(c => Tuple.Create(c.name, c.Id))
          .ToList();
            ViewBag.Companys = companys;
            List<Tuple<string, string, int, int>> fascalyear = _context.fascalYears
            .Select(c => Tuple.Create(c.from.ToString("dd/MM/yyyy"), c.to.ToString("dd/MM/yyyy"), c.Id, c.companyId))
            .ToList();
            ViewBag.fascalyear = fascalyear;
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text");
            ViewData["id"] = id;
            var slabs = await _context.slabs.Include(l => l.company).Include(l => l.fy).ToListAsync();
            var slab = await _context.slabs.FindAsync(id);
            var slabview = new SlabsViewModel
            {
                Slabs = slabs
            };
            if (slab != null)
            {
                slabview = new SlabsViewModel
                {
                    Slabs = slabs,
                    from = slab.from,
                    to = slab.to,
                    percent = slab.percent,
                    extra = slab.extra,
                    fyId = slab.fyId,
                    companyId = slab.companyId
                };
            }
            ViewBag.Modelaction = Modelaction;
            return View(slabview);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,from,to,percent,extra,fyId,companyId")] SlabsModel slabsModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from} - {fy.to}",
                Value = fy.Id.ToString()
            }).ToList();

            try
            {
               
                if (ModelState.IsValid)
                {
                    _context.Add(slabsModel);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
               

                
                ViewData["fyId"] = new SelectList(fYearList, "Value", "Text", slabsModel.fyId);
                return View(slabsModel);
            }
            catch (Exception ex) {
      
                ViewData["fyId"] = new SelectList(fYearList, "Value", "Text", slabsModel.fyId);
                return View(slabsModel);
            }
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,from,to,percent,extra,fyId,companyId")] SlabsModel slabsModel)
        {
            if (id != slabsModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(slabsModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SlabsModelExists(slabsModel.Id))
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
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from} - {fy.to}",
                Value = fy.Id.ToString()
            }).ToList();
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text", slabsModel.fyId);
            return View(slabsModel);
        }
		[Authorize(Roles = "admin,HR")]
		// GET: Company/Delete/5
		[NonAction]
        public IActionResult ConfirmDelete(int id)
        {
            // This method should not be treated as a controller action
            // It can contain shared logic or error handling
            return View();
        }
		[Authorize(Roles = "admin,HR")]

		[HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ConfirmDelete")]
        public async Task<IActionResult> ConfirmDeletePost(int id)
        {
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from} - {fy.to}",
                Value = fy.Id.ToString()
            }).ToList();

            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text");
            var slabs = await _context.slabs.Include(l => l.company).Include(l => l.fy).ToListAsync();
            var slabview = new SlabsViewModel
            {

                Slabs = slabs

            };
            try
            {

                if (_context.slabs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.slabs'  is null.");
            }
            var slabsModel = await _context.slabs.FindAsync(id);
            if (slabsModel != null)
            {
                _context.slabs.Remove(slabsModel);
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
            return View("Index", slabview);
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

        private bool SlabsModelExists(int id)
        {
          return (_context.slabs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
