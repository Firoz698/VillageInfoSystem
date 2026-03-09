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


        // GET: /Admin/TickerEdit/8
        [HttpGet]
        public async Task<IActionResult> TickerEdit(int id)
        {
            var ticker = await _db.TickerNews.FindAsync(id);
            if (ticker == null) return NotFound();
            return View(ticker);
        }

        // POST: /Admin/TickerEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TickerEdit(TickerNews model)
        {
            if (!ModelState.IsValid) return View(model);

            var ticker = await _db.TickerNews.FindAsync(model.Id);
            if (ticker == null) return NotFound();

            ticker.Text = model.Text;
            ticker.SortOrder = model.SortOrder;
            ticker.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            return RedirectToAction("TickerList");
        }

        // ══════════════════════════════════════════════════════════════
        // AdminController.cs — এই methods গুলো যোগ করুন বিদ্যমান controller-এ
        // ══════════════════════════════════════════════════════════════

        // ─── INSTITUTION CRUD ──────────────────────────────────────────────────────
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

        // ─── INSTITUTION MEMBER CRUD ───────────────────────────────────────────────
        [Authorize, HttpGet]
        public IActionResult MemberCreate(int institutionId)
        {
            ViewBag.InstitutionId = institutionId;
            return View(new InstitutionMember { InstitutionId = institutionId });
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> MemberCreate(InstitutionMember member, IFormFile? imageFile)
        {
            ModelState.Remove("Institution");
            if (!ModelState.IsValid)
            {
                ViewBag.InstitutionId = member.InstitutionId;
                return View(member);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "members");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                member.ImagePath = "/uploads/members/" + fileName;
            }

            _db.InstitutionMembers.Add(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সদস্য যোগ করা হয়েছে!";
            return RedirectToAction("InstitutionEdit", new { id = member.InstitutionId });
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> MemberEdit(int id)
        {
            var item = await _db.InstitutionMembers.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> MemberEdit(InstitutionMember member, IFormFile? imageFile)
        {
            ModelState.Remove("Institution");
            ModelState.Remove("ImagePath");
            if (!ModelState.IsValid) return View(member);

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "members");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await imageFile.CopyToAsync(stream);
                member.ImagePath = "/uploads/members/" + fileName;
            }
            else
            {
                var existing = await _db.InstitutionMembers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == member.Id);
                member.ImagePath = existing?.ImagePath;
            }

            _db.InstitutionMembers.Update(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সদস্য আপডেট হয়েছে!";
            return RedirectToAction("InstitutionEdit", new { id = member.InstitutionId });
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> MemberDelete(int id)
        {
            var item = await _db.InstitutionMembers.FindAsync(id);
            int instId = 0;
            if (item != null)
            {
                instId = item.InstitutionId;
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _db.InstitutionMembers.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "সদস্য মুছে ফেলা হয়েছে!";
            return RedirectToAction("InstitutionEdit", new { id = instId });
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


        // ─── FUND USER CRUD ───────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> UserList()
        {
            var list = await _db.Users
                .Include(u => u.Funds)
                .OrderBy(u => u.Name)
                .ToListAsync();
            return View(list);
        }

        [Authorize, HttpGet]
        public IActionResult UserCreate() => View(new VillageInfoSystem.Models.Fund.User());

        [Authorize, HttpPost]
        public async Task<IActionResult> UserCreate(VillageInfoSystem.Models.Fund.User user, IFormFile? profileImage)
        {
            if (!ModelState.IsValid) return View(user);

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "users");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await profileImage.CopyToAsync(stream);
                user.ProfileImage = "/uploads/users/" + fileName;
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সদস্য যোগ করা হয়েছে!";
            return RedirectToAction("UserList");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> UserEdit(int id)
        {
            var item = await _db.Users.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> UserEdit(VillageInfoSystem.Models.Fund.User user, IFormFile? profileImage)
        {
            ModelState.Remove("ProfileImage");
            if (!ModelState.IsValid) return View(user);

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "users");
                Directory.CreateDirectory(uploadPath);
                var fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                using var stream = System.IO.File.Create(Path.Combine(uploadPath, fileName));
                await profileImage.CopyToAsync(stream);
                user.ProfileImage = "/uploads/users/" + fileName;
            }
            else
            {
                var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
                user.ProfileImage = existing?.ProfileImage;
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = "সদস্য আপডেট হয়েছে!";
            return RedirectToAction("UserList");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> UserDelete(int id)
        {
            var item = await _db.Users.FindAsync(id);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.ProfileImage))
                {
                    var filePath = Path.Combine(_env.WebRootPath, item.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _db.Users.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "সদস্য মুছে ফেলা হয়েছে!";
            return RedirectToAction("UserList");
        }

        // ─── FUND CRUD ────────────────────────────────────────────────────────────
        //[Authorize]
        //public async Task<IActionResult> FundList()
        //{
        //    var list = await _db.Funds
        //        .Include(f => f.User)
        //        .OrderByDescending(f => f.Date)
        //        .ToListAsync();
        //    return View(list);
        //}
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

        // ─── DONATION CRUD ────────────────────────────────────────────────────────
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
