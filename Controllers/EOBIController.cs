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
	public class EOBIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public EOBIController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id, string? ModelAction)
        {         
            ViewData["id"] = id;

            List<Tuple<string, int>> companys = _context.companies
            .Select(c => Tuple.Create(c.name, c.Id))
            .ToList();
            ViewBag.companys = companys;
            List<Tuple<string,string, int,int>> fascalyear = _context.fascalYears
            .Select(c => Tuple.Create(c.from.ToString("dd/MM/yyyy"),c.to.ToString("dd/MM/yyyy"), c.Id,c.companyId))
            .ToList();
            ViewBag.fascalyear = fascalyear;
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from.ToString("dd/MM/yyyy")} - {fy.to.ToString("dd/MM/yyyy")}",
                Value = fy.Id.ToString()
            }).ToList();
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text");
            var EOBIs = await _context.EOBIs.Include(l => l.company).Include(l => l.fy).ToListAsync();
            var EOBI = await _context.EOBIs.FindAsync(id);
            var EOBIView = new EOBIViewModel
            {
                EOBI = EOBIs,
                date = null
            };
            if (id != null)
            {
               EOBIView = new EOBIViewModel
                {
                    EOBI = EOBIs,
                    date = EOBI.date,
                    amount = EOBI.amount,
                    fyId = EOBI.fyId,
                    companyId = EOBI.companyId
               };
            }
            ViewBag.ModelAction = ModelAction;
            return View(EOBIView);
        }
		
        [Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,date,amount,companyId,fyId")] EOBIModel EOBIModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(EOBIModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(EOBIModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,date,amount,companyId,fyId")] EOBIModel EOBIModel)
        {
            if (id != EOBIModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(EOBIModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EOBIModelExists(EOBIModel.Id))
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
           
            return View(EOBIModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.EOBIs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.EOBIs'  is null.");
            }
            var EOBIModel = await _context.EOBIs.FindAsync(id);
            if (EOBIModel != null)
            {
                _context.EOBIs.Remove(EOBIModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EOBIModelExists(int id)
        {
          return (_context.EOBIs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
