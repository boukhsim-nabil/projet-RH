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
using AutoMapper.Internal;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class LoanApplyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public LoanApplyController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
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

            ViewBag.LR = (_context.loanApplies.Where(c => c.empId == user.Id && c.status == "Approve" && 
            c.hrstatus == "Approve").Sum(c => c.loanamount))+(_context.loanOpenings.Where(c => c.empId == user.Id)
            .Sum(c => c.opening));

            ViewBag.LP = (_context.payRolls.Where(c => c.employeeId == user.Id).Sum(c => c.loan_deduction));

            ViewBag.RL = ViewBag.LR - ViewBag.LP;


            ViewBag.loanopeingdate = _context.loanOpenings
            .Include(l => l.emp)
            .FirstOrDefault(c => c.empId == user.Id)?.date.ToString("dd MMM yyyy");

            ViewBag.loanopeingamount = _context.loanOpenings
            .Include(l => l.emp)
            .FirstOrDefault(c => c.empId == user.Id)?.opening;

			var loanapplies = await _context.loanApplies
	.Include(l => l.company)
	.Include(l => l.emp)
	.Where(c => c.empId == user.Id && c.hrstatus == status)
	.OrderByDescending(c => c.startdate)
	.ToListAsync();

			if (status == "Pending")
			{
				loanapplies = await _context.loanApplies
	.Include(l => l.company)
	.Include(l => l.emp)
	 .Where(c => c.empId == user.Id && c.hrstatus == "pending" && (c.status == "Approve" || c.status == "Pending"))
		.OrderByDescending(c => c.startdate)
	 .ToListAsync();
			};
			if (status == "Approve")
			{
				loanapplies = await _context.loanApplies
					.Include(l => l.company)
					.Include(l => l.emp)
                    .Where(c => c.empId == user.Id && c.hrstatus == "Approve" && c.status == "Approve")
					.OrderByDescending(c => c.startdate)
	 .ToListAsync();
			};
			if (status == "Reject")
			{
				loanapplies = await _context.loanApplies
					.Include(l => l.company)
					.Include(l => l.emp)
                    .Where(c => c.empId == user.Id && c.status == "Reject" || c.hrstatus == "Reject")
					.OrderByDescending(c => c.startdate)
	 .ToListAsync();
			};


			var loanapplyview = new LoanApplyViewModel 
            {
                loanApply = loanapplies,
                startdate = null
            };
            ViewBag.status = status;
 
            return View(loanapplyview);
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
        public async Task<IActionResult> Create([Bind("Id,startdate,loanamount,repaymentamount,reason,empId,companyId,status,hrstatus")] LoanApplyModel loanApplyModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loanApplyModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["companyId"] = user.companyId;
            ViewData["empId"] = user.Id;
            return View(loanApplyModel);
        }


        public async Task<IActionResult> loan(string fromdate, string todate, string empId)
        {
            List<Tuple<string, int>> companys = _context.companies
.Select(c => Tuple.Create(c.name, c.Id))
.ToList();
            ViewBag.Companys = companys;

            List<Tuple<string, string, int?>> emps = _userManager.Users
.AsEnumerable() // Fetch users into memory
.Where(u => !(_userManager.IsInRoleAsync(u, "HR").Result || _userManager.IsInRoleAsync(u, "Admin").Result)
&& u.status == "Active")
.Select(c => Tuple.Create(c.name, c.Id, c.companyId))
.OrderBy(c => c.Item1)
.ToList();

            ViewBag.emps = emps;
            ViewBag.Fromdate = fromdate;
            var employeeId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(employeeId);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.userRole = userRoles.FirstOrDefault();

            if (empId == "undefined")
            {
                empId = _userManager.GetUserId(User);
            }

            var loan = _context.loanViewModels.FromSqlRaw("EXEC loan_rep @fromdate = '" + fromdate + "',@todate = '" + todate + "',@empId = '" + empId+ "'").ToList();

            foreach (var item in loan)
            {
                _context.Entry(item)
                    .Reference(c => c.emp) // Assuming emp is a reference (single navigation property)
                    .Load();
            }
            return View(loan);
        }
        public IActionResult loanModel()
        {

            List<Tuple<string, string>> emps = _userManager.Users
             .Select(c => Tuple.Create(c.name, c.Id))
             .ToList();
            ViewBag.emps = emps;

            return View();
        }
    }
}
