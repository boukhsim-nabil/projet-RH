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

	public class EmpFamilyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IWebHostEnvironment _webHostEnvironment;


        public EmpFamilyController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment
            , UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

		[Authorize(Roles = "admin,HR")]
        // GET: EmpFamily
		public async Task<IActionResult> Create(string id,int? fid)
        {
            var user = await _userManager.GetUserAsync(User);
            var emp = await _userManager.FindByIdAsync(id);
            ViewData["companyId"] = emp.companyId;
            ViewData["empId"] = id ;
            ViewData["id"] = fid;
            var empfamilies = await _context.empFamily.Include(e => e.company).Include(e => e.emp)
                .Where(c => c.empId == id).ToListAsync();
            var empfamilyDocs = await _context.empFamilyDocs.Where(c => c.empId == id).ToListAsync();
            var empfamily = await _context.empFamily.FindAsync(fid);
            var empfamilyviewmodel = new empFamilyViewModel {
                empFamilyModels = empfamilies,
                empFamilyDocModels = empfamilyDocs,
                DOB = null,
                cnic_expiry = null
            };
            if (empfamily != null)
            {
                empfamilyviewmodel = new empFamilyViewModel
                {
                    empFamilyModels = empfamilies,
                    empFamilyDocModels = empfamilyDocs,
                    name = empfamily.name,
                    relation = empfamily.relation,
                    DOB = empfamily.DOB,
                    cnic = empfamily.cnic,
                    cnic_expiry = empfamily.cnic_expiry,
                    empId = empfamily.empId,
                    companyId = empfamily.companyId 
                    
                };
            }
            return View(empfamilyviewmodel);

            //    return View(await empfamilies.ToListAsync());
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(string? id,[Bind("id,name,relation,DOB,cnic,cnic_expiry,empId,companyId,filepath")] empFamilyViewModel empFamilyModel)
        {
            if (ModelState.IsValid)
            {
                var empFamily = new EmpFamilyModel
                {
                    name = empFamilyModel.name,
                    relation = empFamilyModel.relation,
                    DOB = empFamilyModel.DOB,
                    cnic = empFamilyModel.cnic,
                    cnic_expiry = empFamilyModel.cnic_expiry,
                    empId = empFamilyModel.empId,
                    companyId = empFamilyModel.companyId
                };
                _context.Add(empFamily);
                await _context.SaveChangesAsync();

                string uniqueFileName2 = null;
                if (empFamilyModel.filepath != null && empFamilyModel.filepath.Count > 0)
                {
                    foreach (IFormFile file in empFamilyModel.filepath)
                    {

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        uniqueFileName2 = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName2);
                        file.CopyTo(new FileStream(filePath, FileMode.Create));

                        var empFamilyDoc = new EmpFamilyDocModel
                        {
                            filepath = uniqueFileName2, // Implement this method
                            empId = empFamilyModel.empId,
                            companyId = empFamilyModel.companyId
                        };
                        _context.Add(empFamilyDoc);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Create));
            }
            var user = await _userManager.GetUserAsync(User);
            var emp = await _userManager.FindByIdAsync(id);
            ViewData["companyId"] = emp.companyId;
            ViewData["empId"] = empFamilyModel.Id;
            var empfamilies = await _context.empFamily.Include(e => e.company).Include(e => e.emp)
               .Where(c => c.empId == id).ToListAsync();
            var empfamilyviewmodel = new empFamilyViewModel
            {
                empFamilyModels = empfamilies,
                name = empFamilyModel.name,
                relation = empFamilyModel.relation,
                DOB = empFamilyModel.DOB,
                cnic = empFamilyModel.cnic,
                cnic_expiry = empFamilyModel.cnic_expiry,
                empId = empFamilyModel.empId,
                companyId = empFamilyModel.companyId

            };

            return View(empfamilyviewmodel);
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,string empid, [Bind("Id,name,relation,DOB,cnic,cnic_expiry,empId,companyId,filepath")] empFamilyViewModel empFamilyModel)
        {
            if (id != empFamilyModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var empFamily = new EmpFamilyModel
                    {
                        Id = empFamilyModel.Id,
                        name = empFamilyModel.name,
                        relation = empFamilyModel.relation,
                        DOB = empFamilyModel.DOB,
                        cnic = empFamilyModel.cnic,
                        cnic_expiry = empFamilyModel.cnic_expiry,
                        empId = empFamilyModel.empId,
                        companyId = empFamilyModel.companyId
                    };
                    _context.Update(empFamily);
                    await _context.SaveChangesAsync();
                    string uniqueFileName2 = null;
                    if (empFamilyModel.filepath != null && empFamilyModel.filepath.Count > 0)
                    {
                        foreach (IFormFile file in empFamilyModel.filepath)
                        {

                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }
                            uniqueFileName2 = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName2);
                            file.CopyTo(new FileStream(filePath, FileMode.Create));

                            var empFamilyDoc = new EmpFamilyDocModel
                            {
                                filepath = uniqueFileName2, // Implement this method
                                empId = empFamilyModel.empId,
                                companyId = empFamilyModel.companyId
                            };
                            _context.Add(empFamilyDoc);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpFamilyModelExists(empFamilyModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Create),new {id = empid});
            }
            ViewData["companyId"] = new SelectList(_context.companies, "Id", "name", empFamilyModel.companyId);
            ViewData["empId"] = new SelectList(_context.Users, "Id", "name", empFamilyModel.empId);
            return View(empFamilyModel);
        }


		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id,string empid)
        {
            if (_context.empFamily == null)
            {
                return Problem("Entity set 'ApplicationDbContext.empFamily'  is null.");
            }
            var empFamilyModel = await _context.empFamily.FindAsync(id);
            if (empFamilyModel != null)
            {
                _context.empFamily.Remove(empFamilyModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create), new {id = empid});
        }

		[Authorize(Roles = "admin,HR")]
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoc(int id, string empid)
        {
            if (_context.empFamilyDocs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.empFamilyDocs'  is null.");
            }
            var empFamilyDocsModel = await _context.empFamilyDocs.FindAsync(id);
            if (empFamilyDocsModel != null)
            {
                _context.empFamilyDocs.Remove(empFamilyDocsModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create), new { id = empid });
        }
        // Action to download a document
        public async Task<IActionResult> Download(string filename)
        {

            if (filename == null)
                return Content("filename is not availble");

            var path = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files/", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));

        }
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        // Get mime types
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>   
    {
        {".txt", "text/plain"},
        {".pdf", "application/pdf"},
        {".doc", "application/vnd.ms-word"},
        {".docx", "application/vnd.ms-word"},
        {".xls", "application/vnd.ms-excel"},
        {".pptx", "application/vnd.ms-powerpoint"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".gif", "image/gif"},
        {".csv", "text/csv"}
    };
        }
        private bool EmpFamilyModelExists(int id)
        {
          return (_context.empFamily?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
