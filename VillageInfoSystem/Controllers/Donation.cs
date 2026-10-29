using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;

namespace VillageInfoSystem.Controllers
{
    [Authorize]
    public class Donation : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public Donation(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

        [Authorize]
        public async Task<IActionResult> DonationList()
        {
            var list = await _db.Donations.OrderByDescending(d => d.Date).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult DonationCreate() => View(new VillageInfoSystem.Models.Fund.Donation());

        [Authorize, HttpPost]
        public async Task<IActionResult> DonationCreate(VillageInfoSystem.Models.Fund.Donation donation, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(donation);

            if (image != null && image.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "donations");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await image.CopyToAsync(stream);
                donation.Image = "/uploads/donations/" + fileName;
            }

            _db.Donations.Add(donation);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ডোনেশন যোগ করা হয়েছে!";
            return RedirectToAction("DonationList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> DonationEdit(int id)
        {
            var item = await _db.Donations.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> DonationEdit(VillageInfoSystem.Models.Fund.Donation donation, IFormFile? image)
        {
            ModelState.Remove("Image");
            if (!ModelState.IsValid) return View(donation);

            if (image != null && image.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "donations");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await image.CopyToAsync(stream);
                donation.Image = "/uploads/donations/" + fileName;
            }
            else
            {
                var existing = await _db.Donations.AsNoTracking().FirstOrDefaultAsync(d => d.Id == donation.Id);
                donation.Image = existing?.Image;
            }

            _db.Donations.Update(donation);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ডোনেশন আপডেট হয়েছে!";
            return RedirectToAction("DonationList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> DonationDelete(int id)
        {
            var item = await _db.Donations.FindAsync(id);
            if (item != null) { _db.Donations.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("DonationList");
        }
    }
}
