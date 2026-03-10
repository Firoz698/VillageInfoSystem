using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class Committee : Controller
    {

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public Committee(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

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
    }
}
