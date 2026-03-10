using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class SliderController : Controller
    {

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public SliderController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

        [Authorize]
        public async Task<IActionResult> SliderList()
        {
            var list = await _db.SliderItems.OrderBy(s => s.SortOrder).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult SliderCreate() => View(new SliderItem());

        [Authorize, HttpPost]
        public async Task<IActionResult> SliderCreate(SliderItem item, IFormFile? imageFile)
        {
            ModelState.Remove("ImagePath");

            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "slides");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/slides/" + fileName;
            }

            _db.SliderItems.Add(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "স্লাইডার যোগ করা হয়েছে!";
            return RedirectToAction("SliderList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> SliderEdit(int id)
        {
            var item = await _db.SliderItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> SliderEdit(SliderItem item, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                // পুরনো ইমেজ মুছুন
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "slides");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/slides/" + fileName;
            }

            _db.SliderItems.Update(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "স্লাইডার আপডেট হয়েছে!";
            return RedirectToAction("SliderList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> SliderDelete(int id)
        {
            var item = await _db.SliderItems.FindAsync(id);
            if (item != null)
            {
                // ইমেজ ফাইল মুছুন
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                _db.SliderItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "স্লাইডার মুছে ফেলা হয়েছে!";
            return RedirectToAction("SliderList");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TickerToggle(int id, [FromBody] ToggleDto dto)
        {
            var ticker = await _db.TickerNews.FindAsync(id);
            if (ticker == null) return NotFound();

            ticker.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();

            return Ok();
        }

        public class ToggleDto
        {
            public bool IsActive { get; set; }
        }
    }
}
