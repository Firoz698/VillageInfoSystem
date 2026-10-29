using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    [Authorize]
    public class TickerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public TickerController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }


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

        [HttpGet]
        public async Task<IActionResult> TickerEdit(int id)
        {
            var ticker = await _db.TickerNews.FindAsync(id);
            if (ticker == null) return NotFound();
            return View(ticker);
        }

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


    }
}
