using itgsgroup.Areas.Identity.Data;
using itgsgroup.Models.hrms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class COATRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public COATRequestController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
		[Authorize(Roles = "admin,HOD")]
		public async Task<IActionResult> Index(string status)
        {
            var user = await _userManager.GetUserAsync(User);
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var coats = _context.Correct_AttendTime
             .Include(l => l.company)
             .Include(l => l.emp)
              .Where(c => c.status == status && c.emp.departId == user.departId)
            .OrderByDescending(c => c.correct_datetime)
                .ToList();

            if (role == "admin")
            {
                coats = _context.Correct_AttendTime
                    .Include(l => l.company)
                    .Include(l => l.emp)
                     .Where(c => c.status == status)
                   .OrderByDescending(c => c.correct_datetime)
                    .ToList();
            }
            var coatview = new COATViewModel
            {
                COATModel = coats,
            };
            ViewBag.status = status;
            return View(coatview);
        }
		[Authorize(Roles = "admin,HOD")]
		public IActionResult pendingreq()
        {
            return RedirectToAction(nameof(Index), new { status = "Pending" });
        }
		[Authorize(Roles = "admin,HOD")]
		public IActionResult approvereq()
        {
            return RedirectToAction(nameof(Index), new { status = "Approve" });
        }
		[Authorize(Roles = "admin,HOD")]
		public IActionResult rejectreq()
        {
            return RedirectToAction(nameof(Index), new { status = "Reject" });
        }
		[Authorize(Roles = "admin,HOD")]
		[HttpPost]
        public ActionResult UpdateStatus(int id, string newStatus, string status)
        {
            var coat = _context.Correct_AttendTime.Find(id);

            if (coat != null)
            {
               coat.status = newStatus;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new { status = status });
            }

            return Json(new { success = false, message = "correction of attendence time application not found" });
        }
		[Authorize(Roles = "admin,HR")]
		public async Task<IActionResult> Indexhr(string hrstatus)
        {
            var coat = _context.Correct_AttendTime
                .Include(l => l.company)
                .Include(l => l.emp)
                .Where(c => c.status == "Approve" && c.hrstatus == hrstatus)
                .OrderByDescending(c => c.correct_datetime)
                .ToList();
            var coatview = new COATViewModel
            {
                COATModel = coat,
            };
            ViewBag.hrstatus = hrstatus;
            return View(coatview);
        }
		[Authorize(Roles = "admin,HR")]
		public IActionResult hrpendingreq()
        {
            return RedirectToAction(nameof(Indexhr), new { hrstatus = "Pending" });
        }
		[Authorize(Roles = "admin,HR")]
		public IActionResult hrapprovereq()
        {
            return RedirectToAction(nameof(Indexhr), new { hrstatus = "Approve" });
        }
		[Authorize(Roles = "admin,HR")]
		public IActionResult hrrejectreq()
        {
            return RedirectToAction(nameof(Indexhr), new { hrstatus = "Reject" });
        }
		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        public ActionResult UpdatehrStatus(int id, string newStatus, string hrstatus)
        {
            var coat = _context.Correct_AttendTime.Find(id);

            if (coat != null)
            {
                coat.hrstatus = newStatus;
                _context.SaveChanges();
                return RedirectToAction(nameof(Indexhr), new { hrstatus = hrstatus });
            }

            return Json(new { success = false, message = "correction of attendence time application not found" });
        }

    }
}
