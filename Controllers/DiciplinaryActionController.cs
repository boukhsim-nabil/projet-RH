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
	public class DiciplinaryActionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public DiciplinaryActionController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id, string? ModelAction)
        {
            var user = await _userManager.GetUserAsync(User);
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

            ViewData["id"] = id;
            var diciplinaryactions = await _context.diciplinaryActions.Include(l => l.company)
                .Include(l => l.emp).OrderByDescending(c => c.ActionDate).ToListAsync();
            var diciplinaryaction = await _context.diciplinaryActions.FindAsync(id);
            var diciplinaryactionview = new DiciplinaryActionViewModel 
            {
                diciplnaryAction = diciplinaryactions,
                ActionDate = null
            };
            if (diciplinaryaction != null)
            {
                diciplinaryactionview = new DiciplinaryActionViewModel
                {
                    diciplnaryAction = diciplinaryactions,
                   ActionDate = diciplinaryaction.ActionDate,
                   reason = diciplinaryaction.reason,
                   empId = diciplinaryaction.empId,
                   companyId = diciplinaryaction.companyId
                };
            }
            ViewBag.ModelAction = ModelAction;
            return View(diciplinaryactionview);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActionDate,reason,empId,companyId")] DiciplinaryActionModel diciplinaryActionModel)
        {
            if (ModelState.IsValid)
            {
                var usercompany = _userManager.Users.FirstOrDefault(c => c.Id == diciplinaryActionModel.empId);
                diciplinaryActionModel.companyId = (int)usercompany.companyId;
                _context.Add(diciplinaryActionModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["empId"] = user.Id;
            return View("Index", diciplinaryActionModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActionDate,reason,empId,companyId")] DiciplinaryActionModel diciplinaryActionModel)
        {
            if (id != diciplinaryActionModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usercompany = _userManager.Users.FirstOrDefault(c => c.Id == diciplinaryActionModel.empId);
                    diciplinaryActionModel.companyId = (int)usercompany.companyId;

                    _context.Update(diciplinaryActionModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiciplinaryActionModelExists(diciplinaryActionModel.Id))
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
            ViewData["empId"] = user.Id;
            return View("Index", diciplinaryActionModel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.diciplinaryActions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.diciplinaryActions'  is null.");
            }
            var diciplinaryActionsModel = await _context.diciplinaryActions.FindAsync(id);
            if (diciplinaryActionsModel != null)
            {
                _context.diciplinaryActions.Remove(diciplinaryActionsModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool DiciplinaryActionModelExists(int id)
        {
            return (_context.diciplinaryActions?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
