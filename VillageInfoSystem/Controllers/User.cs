using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;

namespace VillageInfoSystem.Controllers
{
    public class User : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public User(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }


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
    }
}
