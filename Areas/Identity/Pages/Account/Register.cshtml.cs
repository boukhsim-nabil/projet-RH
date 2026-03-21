// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using itgsgroup.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using itgsgroup.Models.hrms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.Identity.Client;
//using AutoMapper;


namespace itgsgroup.Areas.Identity.Pages.Account
{
	[Authorize(Roles = "admin,HR")]
	public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _manager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        public IWebHostEnvironment _webHostEnvironment;
       // private readonly IMapper _mapper;


        public RegisterModel(
            ApplicationDbContext context,
            RoleManager<IdentityRole> manager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment
        //    IMapper mapper
          )
        {
            _context = context;
            _manager = manager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        //    _mapper = mapper;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public string name { get; set; }
            public string? bank { get; set; }
            public string? account { get; set; }
            public string? f_name { get; set; }
            public string? cnic { get; set; }
            public DateTime? cnic_issue { get; set; }
            public DateTime? cnic_expiry { get; set; }
            public string? passport { get; set; }
            public string? curr_address { get; set; }
            public string? permanent_address { get; set; }
            public string? marital_status { get; set; }
            public string? status { get; set; }
            public string? contact { get; set; }
            public string? emergency_contact { get; set; }
            [Required]
            public DateTime? joining_date { get; set; }
            public DateTime? resignation_date { get; set; }
            public string? emp_type { get; set; }
            public int? salary { get; set; }
            public IFormFile? profile_pic { get; set; }
            public string? profile { get; set; }
            public string? attend_type { get; set; }
            public int? companyId { get; set; }
            public int? departId { get; set; }
            public int? designationId { get; set; }
            public int? shiftId { get; set; }
            public int? machineId { get; set; }
            public string? roleId { get; set; }
            public List<IFormFile>? filepath { get; set; }   
            public List<EmpDocModel> empDocs { get; set; }
        }
        public List<EmpFamilyModel> empFamilies { get; set; }
        public List<CompanyModel> Companys { get; set; }
        public List<IdentityRole> roles { get; set; }
        public List<DepartmentModel> departments { get; set; }
        public List<DesignationModel> designations { get; set; }
        public List<ShiftModel> shifts { get; set; }

    
        public async Task OnGetAsync(string returnUrl = null)
        {
            Companys = _context.companies.ToList();
            roles = _manager.Roles.ToList();
            departments = _context.departments.ToList();
            designations = _context.designations.ToList();
            shifts = _context.shift.ToList();
          
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                int? dbuser = await _context.Users.MaxAsync(c => c.empid);
                dbuser ??= 0 ;
                user.empid = dbuser +1;
                user.machineId = Input.machineId;
                user.name = Input.name;
                user.bank = Input.bank;
                user.account = Input.account;
                user.f_name = Input.f_name;
                user.cnic = Input.cnic;
                user.cnic_issue = Input.cnic_issue;
                user.cnic_expiry = Input.cnic_expiry;
                user.passport = Input.passport;
                user.curr_address = Input.curr_address;
                user.permanent_address = Input.permanent_address;
                user.marital_status = Input.marital_status;
                user.status = Input.status;
                user.contact = Input.contact;
                user.emergency_contact = Input.emergency_contact;
                user.joining_date = Input.joining_date;
                user.resignation_date = Input.resignation_date;
                user.emp_type = Input.emp_type;
                user.salary = Input.salary;
                user.attend_type = Input.attend_type;
                string uniqueFileName = null;
                if (Input.profile_pic != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.profile_pic.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Input.profile_pic.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                user.profile_pic = uniqueFileName;
                
                user.companyId = Input.companyId;
                user.departId = Input.departId;
                user.designationId = Input.designationId;
                user.shiftId = Input.shiftId;

               if (Input.roleId != null)
                {
                    var role = await _manager.FindByIdAsync(Input.roleId);
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                
              

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    string uniqueFileName2 = null;
                    if (Input.filepath != null && Input.filepath.Count > 0)
                    {
                        
                        foreach (IFormFile file in Input.filepath)
                        {
                            
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }
                            uniqueFileName2 = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName2);
                            file.CopyTo(new FileStream(filePath, FileMode.Create));
                            var empDoc = new EmpDocModel
                            {
                                filepath = uniqueFileName2,
                                empId = user.Id,
                                companyId = (int)user.companyId
                            
                            };
                            _context.Add(empDoc);
                                               }
                        await _context.SaveChangesAsync();
                     
                    }

                    _logger.LogInformation("User created a new account with password.");

                //    var userId = await _userManager.GetUserIdAsync(user);
                  //  var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                 //   var callbackUrl = Url.Page(
                  //      "/Account/ConfirmEmail",
                 //       pageHandler: null,
                 //       values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                 //       protocol: Request.Scheme);

              //      await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
               //         $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                //    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                //    {
                 //       return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
               //     }
               //     else
                 //   {
                   //     await _signInManager.SignInAsync(user, isPersistent: false);
                        //   return LocalRedirect(returnUrl);
                        //return RedirectToPage("Index", "Employee");
                        return RedirectToAction("Index", "Employee");

                 //   }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            Companys = _context.companies.ToList();
            roles = _manager.Roles.ToList();
            departments = _context.departments.ToList();
            designations = _context.designations.ToList();
            shifts = _context.shift.ToList();
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
