using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class InstitutionC : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public InstitutionC(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }



        [Authorize]
        public async Task<IActionResult> InstitutionList()
        {
            var list = await _db.Institutions
                .Include(i => i.Members)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult InstitutionCreate() => View(new Institution());

        [Authorize, HttpPost]
        public async Task<IActionResult> InstitutionCreate(Institution item, IFormFile? imageFile)
        {
            ModelState.Remove("Members");
            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "institutions");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/institutions/" + fileName;
            }

            _db.Institutions.Add(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "প্রতিষ্ঠান যোগ করা হয়েছে!";
            return RedirectToAction("InstitutionList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> InstitutionEdit(int id)
        {
            var item = await _db.Institutions
                .Include(i => i.Members.OrderBy(m => m.SortOrder))
                .FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> InstitutionEdit(Institution item, IFormFile? imageFile)
        {
            ModelState.Remove("Members");
            ModelState.Remove("ImagePath");
            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "institutions");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/institutions/" + fileName;
            }
            else
            {
                // পুরনো ImagePath রাখো
                var existing = await _db.Institutions.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
                item.ImagePath = existing?.ImagePath;
            }

            _db.Institutions.Update(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "প্রতিষ্ঠান আপডেট হয়েছে!";
            return RedirectToAction("InstitutionList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> InstitutionDelete(int id)
        {
            var item = await _db.Institutions.FindAsync(id);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _db.Institutions.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "প্রতিষ্ঠান মুছে ফেলা হয়েছে!";
            return RedirectToAction("InstitutionList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InstitutionToggleStatus([FromBody] ToggleDtos dto)
        {
            var inst = await _db.Institutions.FindAsync(dto.Id);
            if (inst == null) return NotFound();
            inst.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return Ok();
        }

        public class ToggleDtos
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
