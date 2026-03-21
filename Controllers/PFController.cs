using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using itgsgroup.Areas.Identity.Data;
using itgsgroup.Models.hrms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class PFController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public PFController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id)
        {         
            ViewData["id"] = id;

            List<Tuple<string, int>> companys = _context.companies
            .Select(c => Tuple.Create(c.name, c.Id))
            .ToList();
            ViewBag.companys = companys;
            var PFs = await _context.PFs.Include(l => l.company).ToListAsync();
            var PF = await _context.PFs.FindAsync(id);
            var PFView = new PFViewModel
            {
                PF = PFs,
                date = null
            };
            if (id != null)
            {
               PFView = new PFViewModel
                {
                    PF = PFs,
                    date = PF.date,
                    percent = PF.percent,
                    companyId = PF.companyId
               };
            }

            return View(PFView);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,date,percent,companyId")] PFModel PFModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(PFModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(PFModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,date,percent,companyId")] PFModel PFModel)
        {
            if (id != PFModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(PFModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PFModelExists(PFModel.Id))
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
           
            return View(PFModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.PFs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PFs'  is null.");
            }
            var PFModel = await _context.PFs.FindAsync(id);
            if (PFModel != null)
            {
                _context.PFs.Remove(PFModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PFModelExists(int id)
        {
          return (_context.PFs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
