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
	public class LeaveTypeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaveTypeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Index(int? id,string? ModelAction)
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
            List<Tuple<string, int>> leavename = _context.LeaveNames
          .Select(c => Tuple.Create(c.Name, c.Id))
          .ToList();
            ViewBag.leavenames = leavename;
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text");
            ViewData["id"] = id;
            var leavetypes = await _context.leaveTypes.Include(l => l.company).Include(l => l.fy).ToListAsync();
            var leavetype = await _context.leaveTypes.FindAsync(id);
            var leavetypeview = new leaveTypeViewModel
            {
                leaveTypes = leavetypes
            };
            if (leavetype != null)
            {
                leavetypeview = new leaveTypeViewModel
                {
                    leaveTypes = leavetypes,
                    type = leavetype.type,
                    days = leavetype.days,
                    fyId = leavetype.fyId,
                    companyId = leavetype.companyId
                };
            }
            ViewBag.Modelaction = ModelAction;
            return View(leavetypeview);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,type,days,fyId,companyId")] LeaveTypeModel leaveTypeModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leaveTypeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from} - {fy.to}",
                Value = fy.Id.ToString()
            }).ToList();

            var user = await _userManager.GetUserAsync(User);
            
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text", leaveTypeModel.fyId);
            return View(leaveTypeModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,type,days,fyId,companyId")] LeaveTypeModel leaveTypeModel)
        {
            if (id != leaveTypeModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveTypeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveTypeModelExists(leaveTypeModel.Id))
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
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text", leaveTypeModel.fyId);
            return View("Index",leaveTypeModel);
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
            var fYears = _context.fascalYears.ToList(); // Assuming fyear is your DbSet
            var fYearList = fYears.Select(fy => new SelectListItem
            {
                Text = $"{fy.from} - {fy.to}",
                Value = fy.Id.ToString()
            }).ToList();

            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["fyId"] = new SelectList(fYearList, "Value", "Text");
            var leavetypes = await _context.leaveTypes.Include(l => l.company).Include(l => l.fy).ToListAsync();
            var leavetypeview = new leaveTypeViewModel
            {

                leaveTypes = leavetypes

            };
            try
            {
                if (_context.leaveTypes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.leaveTypes'  is null.");
            }
            var leaveTypeModel = await _context.leaveTypes.FindAsync(id);
            if (leaveTypeModel != null)
            {
                _context.leaveTypes.Remove(leaveTypeModel);
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
            return View("Index", leavetypeview);
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


        private bool LeaveTypeModelExists(int id)
        {
            return (_context.leaveTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }




        // GET: LeaveName
        public async Task<IActionResult> LeavenameIndex(int? id)
        {
            ViewData["id"] = id;

            List<Tuple<string, int>> companys = _context.companies
            .Select(c => Tuple.Create(c.name, c.Id))
            .ToList();
            ViewBag.companys = companys;
            var LeaveNames = await _context.LeaveNames.ToListAsync();
            var LeaveName = await _context.LeaveNames.FindAsync(id);
            var LeaveNameView = new LeaveNameViewModel
            {
                LeaveName = LeaveNames
            };
            if (id != null)
            {
                LeaveNameView = new LeaveNameViewModel
                {
                    LeaveName = LeaveNames,
                    Name = LeaveName.Name
                };
            }

            return View(LeaveNameView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavenameCreate([Bind("Id,Name")] LeaveNameModel LeaveNameModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(LeaveNameModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(LeavenameIndex));
            }
            return View(LeaveNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavenameEdit(int id, [Bind("Id,Name")] LeaveNameModel LeaveNameModel)
        {
            if (id != LeaveNameModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(LeaveNameModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeavenameModelExists(LeaveNameModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(LeavenameIndex));
            }

            return View(LeaveNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavenameDelete(int id)
        {
            if (_context.LeaveNames == null)
            {
                return Problem("Entity set 'ApplicationDbContext.LeaveNames'  is null.");
            }
            var LeaveNameModel = await _context.LeaveNames.FindAsync(id);
            if (LeaveNameModel != null)
            {
                _context.LeaveNames.Remove(LeaveNameModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LeavenameIndex));
        }

        private bool LeavenameModelExists(int id)
        {
            return (_context.LeaveNames?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

