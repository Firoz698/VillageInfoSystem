using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;

namespace VillageInfoSystem.Controllers
{
    public class FundController : Controller
    {

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public FundController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }


        [Authorize]
        public async Task<IActionResult> FundList()
        {
            var list = await _db.Funds
                .Include(f => f.User)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
            var total = list.Sum(f => f.Amount);
            ViewBag.FundList = list;
            ViewBag.TotalFund = total;
            return View();
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> FundCreate()
        {
            ViewBag.Users = await _db.Users.OrderBy(u => u.Name).ToListAsync();
            return View(new VillageInfoSystem.Models.Fund.Fund());
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> FundCreate(VillageInfoSystem.Models.Fund.Fund fund, IFormFile? paymentImage)
        {
            ModelState.Remove("User");
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _db.Users.OrderBy(u => u.Name).ToListAsync();
                return View(fund);
            }

            if (paymentImage != null && paymentImage.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "funds");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(paymentImage.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await paymentImage.CopyToAsync(stream);
                fund.PaymentImage = "/uploads/funds/" + fileName;
            }

            _db.Funds.Add(fund);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ফান্ড এন্ট্রি যোগ করা হয়েছে!";
            return RedirectToAction("FundList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> FundEdit(int id)
        {
            var item = await _db.Funds.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Users = await _db.Users.OrderBy(u => u.Name).ToListAsync();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> FundEdit(VillageInfoSystem.Models.Fund.Fund fund, IFormFile? paymentImage)
        {
            ModelState.Remove("User");
            ModelState.Remove("PaymentImage");
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _db.Users.OrderBy(u => u.Name).ToListAsync();
                return View(fund);
            }

            if (paymentImage != null && paymentImage.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "funds");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(paymentImage.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await paymentImage.CopyToAsync(stream);
                fund.PaymentImage = "/uploads/funds/" + fileName;
            }
            else
            {
                var existing = await _db.Funds.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fund.Id);
                fund.PaymentImage = existing?.PaymentImage;
            }

            _db.Funds.Update(fund);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ফান্ড আপডেট হয়েছে!";
            return RedirectToAction("FundList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> FundDelete(int id)
        {
            var item = await _db.Funds.FindAsync(id);
            if (item != null) { _db.Funds.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("FundList");
        }
    }
}
