using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public NewsController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }
        private async Task<string?> SaveImageAsync(IFormFile? file, string? oldPath = null)
        {
            if (file == null || file.Length == 0) return oldPath;

            // শুধু image file allow করুন
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return oldPath;

            // Max 5MB
            if (file.Length > 5 * 1024 * 1024) return oldPath;

            // পুরনো ছবি মুছুন
            if (!string.IsNullOrEmpty(oldPath))
            {
                var oldFull = Path.Combine(_env.WebRootPath, oldPath.TrimStart('/'));
                if (System.IO.File.Exists(oldFull))
                    System.IO.File.Delete(oldFull);
            }

            // নতুন unique নাম
            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_env.WebRootPath, "uploads", "news", fileName);
            var urlPath = $"/uploads/news/{fileName}";

            using var stream = new FileStream(savePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return urlPath;
        }

        // ── GET: NewsList ──
        // AdminController.cs এ এই action যোগ করুন
        public async Task<IActionResult> NewsList()
        {
            var newsList = await _db.News
                .OrderByDescending(n => n.PublishedAt)
                .ToListAsync();

            return View(newsList);
        }

        // ── POST: NewsDelete ──
        [HttpPost]
        public async Task<IActionResult> NewsDelete(int id)
        {
            var news = await _db.News.FindAsync(id);
            if (news == null) return NotFound();

            // ছবি থাকলে মুছুন
            if (!string.IsNullOrEmpty(news.ImagePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, news.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _db.News.Remove(news);
            await _db.SaveChangesAsync();

            return RedirectToAction("NewsList");
        }

        // ── GET: NewsCreate ──
        [HttpGet]
        public IActionResult NewsCreate() => View(new News());

        // ── POST: NewsCreate ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsCreate(News model)
        {
            if (!ModelState.IsValid) return View(model);

            model.ImagePath = await SaveImageAsync(model.ImageFile);
            model.ImageFile = null;

            _db.News.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("NewsList");
        }

        // ── GET: NewsEdit ──
        [HttpGet]
        public async Task<IActionResult> NewsEdit(int id)
        {
            var news = await _db.News.FindAsync(id);
            if (news == null) return NotFound();
            return View(news);
        }

        // ── POST: NewsEdit ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsEdit(int id, News model)
        {
            if (!ModelState.IsValid) return View(model);

            var news = await _db.News.FindAsync(id);
            if (news == null) return NotFound();

            news.Title = model.Title;
            news.Description = model.Description;
            news.Category = model.Category;
            news.Icon = model.Icon;
            news.PublishedAt = model.PublishedAt;
            news.IsFeatured = model.IsFeatured;
            news.IsActive = model.IsActive;

            // ছবি নতুন দিলে replace, না দিলে পুরনোটা রাখুন
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                news.ImagePath = await SaveImageAsync(model.ImageFile, news.ImagePath);

            await _db.SaveChangesAsync();
            return RedirectToAction("NewsList");
        }

        // ── POST: NewsDeleteImage (ছবি মুছুন) ──
        [HttpPost]
        public async Task<IActionResult> NewsDeleteImage(int id)
        {
            var news = await _db.News.FindAsync(id);
            if (news == null) return NotFound();

            if (!string.IsNullOrEmpty(news.ImagePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, news.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                news.ImagePath = null;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("NewsEdit", new { id });
        }
    }
}
