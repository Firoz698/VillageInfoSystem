using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

        // ─── LOGIN ───────────────────────────────────────────────────────────
        // ── GET ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            // যদি ইতিমধ্যে লগইন করা থাকে, সরাসরি Dashboard-এ পাঠাও
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View();
        }

        // ── POST ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "ইউজারনেম ও পাসওয়ার্ড দেওয়া আবশ্যক!";
                return View();
            }

            const string adminUser = "admin";
            const string adminPass = "admin@123";

            if (username == adminUser && password == adminPass)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProps = new AuthenticationProperties
                {
                    // rememberMe চেক করা থাকলে cookie ৭ দিন টিকবে,
                    // না থাকলে browser বন্ধ হলেই মুছে যাবে
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe? DateTimeOffset.UtcNow.AddDays(7): (DateTimeOffset?)null
                };

                await HttpContext.SignInAsync( CookieAuthenticationDefaults.AuthenticationScheme,principal,authProps);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "ভুল ইউজারনেম বা পাসওয়ার্ড!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.NewsCount = await _db.News.CountAsync();
            ViewBag.FacilityCount = await _db.Facilities.CountAsync();
            ViewBag.CommitteeCount = await _db.CommitteeMembers.CountAsync();
            ViewBag.GalleryCount = await _db.GalleryItems.CountAsync();
            ViewBag.RecentNews = await _db.News.OrderByDescending(n => n.PublishedAt).Take(5).ToListAsync();
            return View();
        }

        // ─── NEWS CRUD ───────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> NewsList()
        {
            var news = await _db.News.OrderByDescending(n => n.PublishedAt).ToListAsync();
            return View(news);
        }

        [Authorize, HttpGet]
        public IActionResult NewsCreate() => View(new News());

        [Authorize, HttpPost]
        public async Task<IActionResult> NewsCreate(News news)
        {
            if (!ModelState.IsValid) return View(news);
            _db.News.Add(news);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সংবাদ সফলভাবে যোগ করা হয়েছে!";
            return RedirectToAction("NewsList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> NewsEdit(int id)
        {
            var news = await _db.News.FindAsync(id);
            if (news == null) return NotFound();
            return View(news);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> NewsEdit(News news)
        {
            if (!ModelState.IsValid) return View(news);
            _db.News.Update(news);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সংবাদ সফলভাবে আপডেট হয়েছে!";
            return RedirectToAction("NewsList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> NewsDelete(int id)
        {
            var news = await _db.News.FindAsync(id);
            if (news != null) { _db.News.Remove(news); await _db.SaveChangesAsync(); }
            TempData["Success"] = "সংবাদ মুছে ফেলা হয়েছে!";
            return RedirectToAction("NewsList");
        }

        // ─── FACILITY CRUD ────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> FacilityList()
        {
            var list = await _db.Facilities.OrderBy(f => f.SortOrder).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult FacilityCreate() => View(new Facility());

        [Authorize, HttpPost]
        public async Task<IActionResult> FacilityCreate(Facility facility)
        {
            if (!ModelState.IsValid) return View(facility);
            _db.Facilities.Add(facility);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সুবিধা সফলভাবে যোগ করা হয়েছে!";
            return RedirectToAction("FacilityList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> FacilityEdit(int id)
        {
            var item = await _db.Facilities.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> FacilityEdit(Facility facility)
        {
            if (!ModelState.IsValid) return View(facility);
            _db.Facilities.Update(facility);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সফলভাবে আপডেট হয়েছে!";
            return RedirectToAction("FacilityList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> FacilityDelete(int id)
        {
            var item = await _db.Facilities.FindAsync(id);
            if (item != null) { _db.Facilities.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("FacilityList");
        }

        // ─── COMMITTEE CRUD ───────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> CommitteeList()
        {
            var list = await _db.CommitteeMembers.OrderBy(c => c.SortOrder).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult CommitteeCreate() => View(new CommitteeMember());

        [Authorize, HttpPost]
        public async Task<IActionResult> CommitteeCreate(CommitteeMember member)
        {
            if (!ModelState.IsValid) return View(member);
            _db.CommitteeMembers.Add(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সদস্য সফলভাবে যোগ করা হয়েছে!";
            return RedirectToAction("CommitteeList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> CommitteeEdit(int id)
        {
            var item = await _db.CommitteeMembers.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> CommitteeEdit(CommitteeMember member)
        {
            if (!ModelState.IsValid) return View(member);
            _db.CommitteeMembers.Update(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সফলভাবে আপডেট হয়েছে!";
            return RedirectToAction("CommitteeList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> CommitteeDelete(int id)
        {
            var item = await _db.CommitteeMembers.FindAsync(id);
            if (item != null) { _db.CommitteeMembers.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("CommitteeList");
        }

        // ─── GALLERY CRUD ─────────────────────────────────────────────────────
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

        // ─── TICKER NEWS ──────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> TickerList()
        {
            var list = await _db.TickerNews.OrderBy(t => t.SortOrder).ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult TickerCreate() => View(new TickerNews());

        [Authorize, HttpPost]
        public async Task<IActionResult> TickerCreate(TickerNews ticker)
        {
            if (!ModelState.IsValid) return View(ticker);
            _db.TickerNews.Add(ticker);
            await _db.SaveChangesAsync();
            TempData["Success"] = "টিকার যোগ করা হয়েছে!";
            return RedirectToAction("TickerList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> TickerDelete(int id)
        {
            var item = await _db.TickerNews.FindAsync(id);
            if (item != null) { _db.TickerNews.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "মুছে ফেলা হয়েছে!";
            return RedirectToAction("TickerList");
        }

        // ─── VILLAGE INFO ──────────────────────────────────────────────────────
        [Authorize, HttpGet]
        public async Task<IActionResult> VillageInfoEdit()
        {
            var info = await _db.VillageInfos.FirstOrDefaultAsync() ?? new VillageInfo();
            return View(info);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> VillageInfoEdit(VillageInfo info)
        {
            if (!ModelState.IsValid) return View(info);
            if (info.Id == 0)
                _db.VillageInfos.Add(info);
            else
                _db.VillageInfos.Update(info);
            await _db.SaveChangesAsync();
            TempData["Success"] = "গ্রামের তথ্য আপডেট হয়েছে!";
            return RedirectToAction("Dashboard");
        }

        // ─── SLIDER CRUD ──────────────────────────────────────────────────────
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
    }
}
