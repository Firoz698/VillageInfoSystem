using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class FacilityC : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public FacilityC(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }


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

    }
}
