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
	public class LoanOpeningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public LoanOpeningController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
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
            /*	List<Tuple<string, string, int?>> emps = _userManager.Users
                .AsEnumerable() // Fetch users into memory
                .Where(u => !(_userManager.IsInRoleAsync(u, "HR").Result || _userManager.IsInRoleAsync(u, "Admin").Result)
                && u.status == "Active")
                .Select(c => Tuple.Create(c.name, c.Id, c.companyId))
                .OrderBy(c => c.Item1)
                .ToList();
            */
            var hrAndAdminRoleIds = _context.Roles
                 .Where(r => r.Name == "HR" || r.Name == "Admin")
                 .Select(r => r.Id)
                 .ToList();

            // Fetch user IDs associated with those roles
            var hrAndAdminUserIds = _context.UserRoles
                .Where(ur => hrAndAdminRoleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .ToList();

            // Filter users who are NOT in "HR" or "Admin" and are "Active"
            var emps = _context.Users
                .Where(u => !hrAndAdminUserIds.Contains(u.Id) && u.status == "Active")
                .OrderBy(u => u.name)
                .Select(u => Tuple.Create(u.name, u.Id, u.companyId))
                .ToList();
            ViewBag.emps = emps;
            List<Tuple<string, int>> companys = _context.companies
            .Select(c => Tuple.Create(c.name, c.Id))
            .ToList();
            ViewBag.companys = companys;
            var loanOpenings = await _context.loanOpenings.Include(l => l.emp).ToListAsync();
            var loanOpening = await _context.loanOpenings.FindAsync(id);
            var loanOpeningView = new LoanOpeningViewModel
            {
                loanopening = loanOpenings,
                date = null
            };
            if (id != null)
            {
               loanOpeningView = new LoanOpeningViewModel
                {
                    loanopening = loanOpenings,
                    date = loanOpening.date,
                    opening = loanOpening.opening,
                    empId = loanOpening.empId
               };
            }
            ViewBag.ModelAction = ModelAction;
            return View(loanOpeningView);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,date,opening,empId")] LoanOpeningModel loanOpeningModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loanOpeningModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loanOpeningModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,date,opening,empId")] LoanOpeningModel loanOpeningModel)
        {
            if (id != loanOpeningModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loanOpeningModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanOpeningModelExists(loanOpeningModel.Id))
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
           
            return View(loanOpeningModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.loanOpenings == null)
            {
                return Problem("Entity set 'ApplicationDbContext.loanOpenings'  is null.");
            }
            var loanOpeningModel = await _context.loanOpenings.FindAsync(id);
            if (loanOpeningModel != null)
            {
                _context.loanOpenings.Remove(loanOpeningModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoanOpeningModelExists(int id)
        {
          return (_context.loanOpenings?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
