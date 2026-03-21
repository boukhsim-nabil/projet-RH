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
	public class SandwichAttController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public SandwichAttController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id,string? ModelAction)
        {         
            ViewData["id"] = id;

            /*	List<Tuple<string, string, int?>> emps = _userManager.Users
         .AsEnumerable() // Fetch users into memory
         .Where(u => !(_userManager.IsInRoleAsync(u, "HR").Result || _userManager.IsInRoleAsync(u, "Admin").Result)
         && u.status == "Active")
         .Select(c => Tuple.Create(c.name, c.Id, c.companyId))
         .OrderBy(c => c.Item1)
         .ToList();*/
            // Fetch all "HR" and "Admin" role IDs
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
            var sandwiches = await _context.sandwichAtts.Include(l => l.emp).OrderByDescending(c => c.date).ToListAsync();
            var sandwiche = await _context.sandwichAtts.FindAsync(id);
            var sandwichattView = new SandwichAttViewModel
            {
                sandwichatt = sandwiches,
                from = null,
                to = null
            };
            if (id != null)
            {
               sandwichattView = new SandwichAttViewModel
                {
                    sandwichatt = sandwiches,
                    date = sandwiche.date,
                    from = sandwiche.from,
                    to = sandwiche.to,
                    reason = sandwiche.reason,
                    empId = sandwiche.empId
               };
            }
            ViewBag.ModelAction = ModelAction;
            return View(sandwichattView);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,date,from,to,reason,empId")] SandwichAttModel SandwichAttModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(SandwichAttModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(SandwichAttModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,date,from,to,reason,empId")] SandwichAttModel sandwichAttModel)
        {
            if (id != sandwichAttModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sandwichAttModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SandwichAttModelExists(sandwichAttModel.Id))
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
           
            return View(sandwichAttModel);
        }



		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.sandwichAtts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.sandwichAtts'  is null.");
            }
            var sandwichAttModel = await _context.sandwichAtts.FindAsync(id);
            if (sandwichAttModel != null)
            {
                _context.sandwichAtts.Remove(sandwichAttModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SandwichAttModelExists(int id)
        {
          return (_context.sandwichAtts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
