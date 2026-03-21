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
	public class COATController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public COATController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: LeaveApply
        public async Task<IActionResult> Index(string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.userRole = userRoles.FirstOrDefault();

            ViewData["companyId"] = user.companyId;
            ViewData["empId"] = user.Id;
			
            ViewBag.yearlyTC = _context.Correct_AttendTime.Count(c => c.correct_datetime.Year == DateTime.Now.Year 
            && c.empId == user.Id);
			
            ViewBag.monthlyTC = _context.Correct_AttendTime.Count(c => c.correct_datetime.Year == DateTime.Now.Year
            && c.correct_datetime.Month == DateTime.Now.Month && c.empId == user.Id);
			
            DateTime today = DateTime.Now;
			int daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
			DateTime startOfWeek = today.AddDays(-daysUntilMonday);
			DateTime endOfWeek = startOfWeek.AddDays(7); // Calculate the end of the current week
			
            ViewBag.weeklyTC = _context.Correct_AttendTime.Count(c =>
				c.correct_datetime >= startOfWeek && c.correct_datetime < endOfWeek && c.empId == user.Id);
			
            ViewBag.pendingTC = _context.Correct_AttendTime.Count(c =>
				c.hrstatus == "Pending" && c.empId == user.Id);
			
            var coats = await _context.Correct_AttendTime.Include(l => l.company)
                .Include(l => l.emp).Where(c => c.empId == user.Id &&  c.hrstatus == status).ToListAsync();

			if (status == "Pending")
			{
				coats = await _context.Correct_AttendTime.Include(l => l.company)
					.Include(l => l.emp)
	 .Where(c => c.empId == user.Id && c.hrstatus == "pending" && (c.status == "Approve" || c.status == "Pending"))
	 .ToListAsync();
			};
			if (status == "Approve")
			{
				coats = await _context.Correct_AttendTime.Include(l => l.company)
					 .Include(l => l.emp)
	 .Where(c => c.empId == user.Id && c.hrstatus == "Approve" && c.status == "Approve")
	 .ToListAsync();
			};
			if (status == "Reject")
			{
				coats = await _context.Correct_AttendTime.Include(l => l.company)
					 .Include(l => l.emp)
	 .Where(c => c.empId == user.Id && c.status == "Reject" || c.hrstatus == "Reject")
	 .ToListAsync();
			};

			var coatview = new COATViewModel 
            {
                COATModel = coats,
                correct_datetime = null
            };
            ViewBag.status = status;

            return View(coatview);
        }
		public IActionResult pendingreq()
		{
			return RedirectToAction(nameof(Index), new { status = "Pending" });
		}
		public IActionResult approvereq()
		{
			return RedirectToAction(nameof(Index), new { status = "Approve" });
		}
		public IActionResult rejectreq()
		{
			return RedirectToAction(nameof(Index), new { status = "Reject" });
		}

		// GET: LeaveApply/Details/5

		// GET: LeaveApply/Create

		// POST: LeaveApply/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,date,correct_datetime,reason,empId,companyId,status,hrstatus")] COATModel COATModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(COATModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["empId"] = user.Id;
            return View(COATModel);
        }

    }
}
