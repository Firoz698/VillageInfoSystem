using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class Gallery : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public Gallery(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }


        [Authorize]
        public async Task<IActionResult> GalleryList()
        {
            var list = await _db.GalleryItems.OrderBy(g => g.SortOrder).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult GalleryCreate() => View(new GalleryItem());

        [Authorize, HttpPost]
        public async Task<IActionResult> GalleryCreate(GalleryItem item, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/" + fileName;
            }

            _db.GalleryItems.Add(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "গ্যালারি আইটেম যোগ করা হয়েছে!";
            return RedirectToAction("GalleryList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> GalleryEdit(int id)
        {
            var item = await _db.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> GalleryEdit(GalleryItem item, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(item);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                item.ImagePath = "/uploads/" + fileName;
            }

            _db.GalleryItems.Update(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সফলভাবে আপডেট হয়েছে!";
            return RedirectToAction("GalleryList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> GalleryDelete(int id)
        {
            var item = await _db.GalleryItems.FindAsync(id);
            if (item != null) { _db.GalleryItems.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("GalleryList");
        }
    }
}
