using iTextSharp.text;
using iTextSharp.text.pdf;
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



        [Authorize, HttpGet]
        public async Task<IActionResult> FundReport(string? month, DateTime? from, DateTime? to, int? userId = null)
        {
            var userName = User.Identity?.Name ?? "Admin";

            var fundsQuery = _db.Funds.AsNoTracking().Include(f => f.User).AsQueryable();

            if (userId.HasValue && userId.Value > 0)
                fundsQuery = fundsQuery.Where(x => x.UserId == userId.Value);

            if (from.HasValue || to.HasValue)
            {
                if (from.HasValue) fundsQuery = fundsQuery.Where(x => x.Date >= from.Value);
                if (to.HasValue) fundsQuery = fundsQuery.Where(x => x.Date <= to.Value);
            }
            else if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParse($"{month}-01", out var sel))
                {
                    var ms2 = new DateTime(sel.Year, sel.Month, 1);
                    var me = ms2.AddMonths(1).AddDays(-1);
                    fundsQuery = fundsQuery.Where(x => x.Date >= ms2 && x.Date <= me);
                }
            }
            else
            {
                var cm = DateTime.Now.Month;
                var cy = DateTime.Now.Year;
                fundsQuery = fundsQuery.Where(x => x.Date.Month == cm && x.Date.Year == cy);
            }

            var list = await fundsQuery.OrderByDescending(x => x.Date).ToListAsync();
            if (!list.Any())
            {
                TempData["Error"] = "নির্বাচিত ফিল্টারে কোনো ডেটা পাওয়া যায়নি।";
                return RedirectToAction("FundList");
            }

            var allUsers = await _db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.Name);

            string GetMemberName(VillageInfoSystem.Models.Fund.Fund f)
            {
                if (!string.IsNullOrEmpty(f.User?.Name)) return f.User.Name;
                if (allUsers.TryGetValue(f.UserId, out var n)) return n;
                return "-";
            }

            var byUser = list
                .GroupBy(x => GetMemberName(x))
                .Select(g => new { Person = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Person).ToList();

            var byMonth = list
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new { Label = $"{g.Key.Month}/{g.Key.Year}", Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Label).ToList();

            decimal grandTotal = list.Sum(x => x.Amount);

            // ✅ TransactionReport-এর হুবহু একই pattern
            using var memStream = new MemoryStream();
            var doc = new Document(PageSize.A4, 20f, 20f, 40f, 60f);
            var writer = PdfWriter.GetInstance(doc, memStream);
            writer.PageEvent = new FundPdfFooter(userName);
            doc.Open();

            // ── Fonts ──
            var fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
            if (!System.IO.File.Exists(fontPath))
                fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");

            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var normalFont = new Font(baseFont, 10);
            var boldFont = new Font(baseFont, 12, Font.BOLD);
            var headerFont = new Font(baseFont, 20, Font.BOLD);
            var greenFont = new Font(baseFont, 10, Font.NORMAL, new BaseColor(13, 92, 46));
            var whiteFont = new Font(baseFont, 12, Font.BOLD, BaseColor.White);

            // ── Header ──
            var logoPath = Path.Combine(_env.WebRootPath, "Logo", "logo.jpg");
            var headerTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 10f };
            headerTable.SetWidths(new float[] { 15f, 85f });

            if (System.IO.File.Exists(logoPath))
            {
                var logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(50f, 50f);
                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase(""))
                { Border = Rectangle.NO_BORDER });
            }

            headerTable.AddCell(new PdfPCell(new Phrase("Fund Report", headerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            });
            doc.Add(headerTable);
            doc.Add(new Paragraph("\n"));

            // ── Subtitle ──
            var filterParts = new List<string>();
            if (userId.HasValue && userId > 0)
                filterParts.Add($"Member: {GetMemberName(list.First())}");
            if (!string.IsNullOrEmpty(month))
                filterParts.Add($"Month: {month}");
            if (from.HasValue && to.HasValue)
                filterParts.Add($"Date: {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}");

            var subtitleText = $"Total Entries: {list.Count}" +
                (filterParts.Count > 0 ? $" (Filters – {string.Join(", ", filterParts)})" : "");
            doc.Add(new Paragraph(subtitleText, normalFont)
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 12f });

            // ── Member-wise Summary ──
            doc.Add(new Paragraph("Member-wise Summary", boldFont) { SpacingAfter = 5f });
            var summaryTable = new PdfPTable(2)
            { WidthPercentage = 55f, SpacingAfter = 15f, HorizontalAlignment = Element.ALIGN_LEFT };
            summaryTable.SetWidths(new float[] { 60f, 40f });
            summaryTable.AddCell(new PdfPCell(new Phrase("Member", boldFont))
            { Padding = 5f, BackgroundColor = BaseColor.LightGray });
            summaryTable.AddCell(new PdfPCell(new Phrase("Total Amount", boldFont))
            {
                Padding = 5f,
                BackgroundColor = BaseColor.LightGray,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            foreach (var s in byUser)
            {
                summaryTable.AddCell(new PdfPCell(new Phrase(s.Person, normalFont)) { Padding = 5f });
                summaryTable.AddCell(new PdfPCell(new Phrase(s.Total.ToString("0.00") + " ৳", greenFont))
                { Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            doc.Add(summaryTable);

            // ── Monthly Summary ──
            doc.Add(new Paragraph("Monthly Summary", boldFont) { SpacingAfter = 5f });
            var monthTable = new PdfPTable(2)
            { WidthPercentage = 45f, SpacingAfter = 15f, HorizontalAlignment = Element.ALIGN_LEFT };
            monthTable.SetWidths(new float[] { 55f, 45f });
            monthTable.AddCell(new PdfPCell(new Phrase("Month", boldFont))
            { Padding = 5f, BackgroundColor = BaseColor.LightGray });
            monthTable.AddCell(new PdfPCell(new Phrase("Total Amount", boldFont))
            {
                Padding = 5f,
                BackgroundColor = BaseColor.LightGray,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            foreach (var m in byMonth)
            {
                monthTable.AddCell(new PdfPCell(new Phrase(m.Label, normalFont)) { Padding = 5f });
                monthTable.AddCell(new PdfPCell(new Phrase(m.Total.ToString("0.00") + " ৳", greenFont))
                { Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            doc.Add(monthTable);

            // ── Grand Total ──
            var totalTable = new PdfPTable(2)
            { WidthPercentage = 40f, SpacingAfter = 15f, HorizontalAlignment = Element.ALIGN_LEFT };
            totalTable.SetWidths(new float[] { 60f, 40f });
            totalTable.AddCell(new PdfPCell(new Phrase("Grand Total",
                new Font(baseFont, 12, Font.BOLD, BaseColor.White)))
            { Padding = 6f, BackgroundColor = new BaseColor(13, 92, 46) });
            totalTable.AddCell(new PdfPCell(new Phrase(grandTotal.ToString("0.00") + " ৳", whiteFont))
            {
                Padding = 6f,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = new BaseColor(13, 92, 46)
            });
            doc.Add(totalTable);

            // ── Detail Table ──
            doc.Add(new Paragraph("All Fund Entries", boldFont) { SpacingAfter = 5f });
            var table = new PdfPTable(5) { WidthPercentage = 100f };
            table.SetWidths(new float[] { 5f, 20f, 25f, 30f, 20f });

            foreach (var h in new[] { "SL", "Date", "Member", "Amount", "Payment" })
                table.AddCell(new PdfPCell(new Phrase(h, boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = BaseColor.LightGray,
                    Padding = 5f
                });

            int sl = 1;
            foreach (var f in list)
            {
                var mn = GetMemberName(f);
                table.AddCell(new PdfPCell(new Phrase(sl.ToString(), normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(f.Date.ToString("dd-MMM-yyyy"), normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(mn, normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(f.Amount.ToString("0.00") + " ৳", greenFont))
                { Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(
                    string.IsNullOrEmpty(f.PaymentImage) ? "N/A" : "Uploaded", normalFont))
                { Padding = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                sl++;
            }

            doc.Add(table);
            doc.Close();

            return File(memStream.ToArray(), "application/pdf", "FundReport.pdf");
        }

        public class FundPdfFooter : PdfPageEventHelper
        {
            private readonly string _userName;
            public FundPdfFooter(string userName) { _userName = userName; }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var font = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL, BaseColor.Gray);
                var cb = writer.DirectContent;
                float yPos = document.PageSize.GetBottom(30);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                    new Phrase("Developed by Md Firoz Ali", font),
                    document.LeftMargin, yPos, 0);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase($"Report generated by {_userName}", font),
                    document.PageSize.Width / 2, yPos, 0);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                    new Phrase($"Page {writer.PageNumber}", font),
                    document.PageSize.Width - document.RightMargin, yPos, 0);
            }
        }


    }
}