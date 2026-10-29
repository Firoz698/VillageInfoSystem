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
            ViewBag.InstitutionCount = await _db.Institutions.CountAsync();
            ViewBag.UserCount = await _db.Users.CountAsync();
            ViewBag.SliderCount = await _db.SliderItems.CountAsync();
            ViewBag.TickerCount = await _db.TickerNews.CountAsync();

            var totalFund = await _db.Funds.SumAsync(f => (decimal?)f.Amount) ?? 0m;
            var totalDonation = await _db.Donations.SumAsync(d => (decimal?)d.Amount) ?? 0m;
            ViewBag.TotalFund = totalFund;
            ViewBag.TotalDonation = totalDonation;
            ViewBag.NetBalance = totalFund - totalDonation;

            ViewBag.RecentNews = await _db.News.OrderByDescending(n => n.PublishedAt).Take(5).ToListAsync();
            ViewBag.RecentFunds = await _db.Funds.Include(f => f.User).OrderByDescending(f => f.Date).Take(5).ToListAsync();

            return View();
        }


        // ─── TICKER NEWS ──────────────────────────────────────────────────────



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
            return RedirectToAction("InstitutionEdit", "Institution", new { id = member.InstitutionId });
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
            return RedirectToAction("InstitutionEdit", "Institution", new { id = member.InstitutionId });
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
            return RedirectToAction("InstitutionEdit", "Institution", new { id = instId });
        }




    }
}
