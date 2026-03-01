using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var info = await _db.VillageInfos.FirstOrDefaultAsync();
            var featuredNews = await _db.News
                .Where(n => n.IsActive && n.IsFeatured)
                .OrderByDescending(n => n.PublishedAt)
                .FirstOrDefaultAsync();
            var newsList = await _db.News
                .Where(n => n.IsActive && !n.IsFeatured)
                .OrderByDescending(n => n.PublishedAt)
                .Take(5)
                .ToListAsync();
            var facilities = await _db.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder)
                .ToListAsync();
            var committee = await _db.CommitteeMembers
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            var gallery = await _db.GalleryItems
                .Where(g => g.IsActive)
                .OrderBy(g => g.SortOrder)
                .ToListAsync();
            var tickers = await _db.TickerNews
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            ViewBag.Info = info;
            ViewBag.FeaturedNews = featuredNews;
            ViewBag.NewsList = newsList;
            ViewBag.Facilities = facilities;
            ViewBag.Committee = committee;
            ViewBag.Gallery = gallery;
            ViewBag.Tickers = tickers;

            return View();
        }

        public IActionResult Error() => View();
    }
}
