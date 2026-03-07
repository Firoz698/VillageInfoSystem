using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Data;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var info = await _db.VillageInfos.FirstOrDefaultAsync();
            var featuredNews = await _db.News.Where(n => n.IsActive && n.IsFeatured).OrderByDescending(n => n.PublishedAt).FirstOrDefaultAsync();
            var newsList = await _db.News.Where(n => n.IsActive && !n.IsFeatured).OrderByDescending(n => n.PublishedAt).Take(6).ToListAsync();
            var facilities = await _db.Facilities.Where(f => f.IsActive).OrderBy(f => f.SortOrder).ToListAsync();
            var committee = await _db.CommitteeMembers.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();
            var gallery = await _db.GalleryItems.Where(g => g.IsActive).OrderBy(g => g.SortOrder).ToListAsync();
            var tickers = await _db.TickerNews.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();
            var sliders = await _db.SliderItems.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync();
            var institutions = await _db.Institutions.Where(i => i.IsActive).OrderBy(i => i.SortOrder).ToListAsync();

            ViewBag.Info = info;
            ViewBag.FeaturedNews = featuredNews;
            ViewBag.NewsList = newsList;
            ViewBag.Facilities = facilities;
            ViewBag.Committee = committee;
            ViewBag.Gallery = gallery;
            ViewBag.Tickers = tickers;
            ViewBag.Sliders = sliders;
            ViewBag.Institutions = institutions;
            return View();
        }

        // All News Page (with category filter + pagination)
        public async Task<IActionResult> AllNews(string? category, int page = 1)
        {
            const int pageSize = 9;

            var query = _db.News.Where(n => n.IsActive);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(n => n.Category == category);

            var totalCount = await query.CountAsync();
            var newsList = await query
                .OrderByDescending(n => n.IsFeatured)
                .ThenByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _db.News
                .Where(n => n.IsActive)
                .Select(n => n.Category)
                .Distinct()
                .ToListAsync();

            var info = await _db.VillageInfos.FirstOrDefaultAsync();

            ViewBag.Info = info;
            ViewBag.NewsList = newsList;
            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            return View();
        }

        // News Details Page
        public async Task<IActionResult> NewsDetails(int id)
        {
            var news = await _db.News.FirstOrDefaultAsync(n => n.Id == id && n.IsActive);
            if (news == null) return NotFound();

            var relatedNews = await _db.News
                .Where(n => n.IsActive && n.Id != id && n.Category == news.Category)
                .OrderByDescending(n => n.PublishedAt)
                .Take(5)
                .ToListAsync();

            if (relatedNews.Count < 3)
            {
                var extraIds = relatedNews.Select(r => r.Id).ToList();
                var extra = await _db.News
                    .Where(n => n.IsActive && n.Id != id && !extraIds.Contains(n.Id))
                    .OrderByDescending(n => n.PublishedAt)
                    .Take(5 - relatedNews.Count)
                    .ToListAsync();
                relatedNews.AddRange(extra);
            }

            ViewBag.RelatedNews = relatedNews;
            return View(news);
        }

        // Institution Details Page
        public async Task<IActionResult> InstitutionDetails(int id)
        {
            var institution = await _db.Institutions
                .Include(i => i.Members.OrderBy(m => m.SortOrder))
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);
            if (institution == null) return NotFound();

            var info = await _db.VillageInfos.FirstOrDefaultAsync();
            ViewBag.Info = info;
            return View(institution);
        }

        public IActionResult Error() => View();
    }
}
