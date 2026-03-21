using itgsgroup.Areas.Identity.Data;
using itgsgroup.Models.hrms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Reflection.Metadata.BlobBuilder;

namespace itgsgroup.Controllers
{
	[Authorize]
	public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _manager;
        private readonly UserManager<ApplicationUser> userManager;

        public RolesController(RoleManager<IdentityRole> rolemanager,
            UserManager<ApplicationUser> userManager)
        {
            _manager = rolemanager;
            this.userManager = userManager;
        }
		[Authorize(Roles = "admin,HR")]
		public IActionResult Index()
        {
            var slabview = new roleModel
            {
                identityRoles = _manager.Roles
        };
            return View(slabview);
        }
		[Authorize(Roles = "admin,HR")]
		public IActionResult Create()
        {
            return View();
        }
		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        public IActionResult Create(IdentityRole role)
        {
            if (!_manager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
            {
                _manager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index");
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _manager.FindByNameAsync(roleName);

            if (role != null)
            {
                var result = await _manager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Roles"); // Redirect to a page where you list roles.
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Role not found.");
            }
            return View("Error"); // You can create an error view to display error messages.
        }

    }
}
