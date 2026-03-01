using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VillageInfoSystem.Models;

namespace VillageInfoSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<News> News => Set<News>();
        public DbSet<Facility> Facilities => Set<Facility>();
        public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
        public DbSet<VillageInfo> VillageInfos => Set<VillageInfo>();
        public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();
        public DbSet<TickerNews> TickerNews => Set<TickerNews>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Village Info
            modelBuilder.Entity<VillageInfo>().HasData(new VillageInfo
            {
                Id = 1,
                VillageName = "উজালপুর গ্রাম",
                Division = "খুলনা বিভাগ",
                History = "উজালপুর গ্রাম খুলনা বিভাগের একটি সমৃদ্ধ ও ঐতিহ্যবাহী গ্রাম। আনুমানিক ১৮৫০ সালে এই গ্রামের পত্তন হয়। শতাব্দীর পর শতাব্দী ধরে এই গ্রাম তার নিজস্ব সংস্কৃতি ও ঐতিহ্য বজায় রেখে আসছে।",
                TotalPopulation = "৩,৫০০+",
                TotalFamilies = "৭৫০+",
                Area = "৪.২ কিমি²",
                MalePopulation = "১,৮৫০",
                FemalePopulation = "১,৬৫০",
                LiteracyRate = "৭৮%",
                FoundedYear = "১৮৫০",
                Address = "উজালপুর গ্রাম, খুলনা বিভাগ, বাংলাদেশ",
                Phone = "০১৭১২-৩৪৫৬৭৮",
                Email = "ujalpur.village@gmail.com",
                Facebook = "fb.com/UjalpurVillage",
                OfficeHours = "সকাল ৯টা - বিকাল ৫টা (শুক্রবার বন্ধ)",
                MapsLink = "https://maps.google.com"
            });

            // Seed News
            modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Title = "উজালপুর গ্রামে নতুন প্রাথমিক বিদ্যালয় ভবন নির্মাণ কাজ শুরু", Description = "দীর্ঘদিনের দাবির প্রেক্ষিতে অবশেষে উজালপুর সরকারি প্রাথমিক বিদ্যালয়ের নতুন দোতলা ভবন নির্মাণ কাজ শুরু হয়েছে। এই ভবনে ১২টি শ্রেণিকক্ষ এবং একটি কম্পিউটার ল্যাব থাকবে।", Category = "শিক্ষা", Icon = "🏫", IsFeatured = true, PublishedAt = new DateTime(2026, 2, 28) },
                new News { Id = 2, Title = "গ্রামের সড়ক উন্নয়নে ৫০ লক্ষ টাকা বরাদ্দ অনুমোদন", Category = "উন্নয়ন", Icon = "🛣️", PublishedAt = new DateTime(2026, 2, 25) },
                new News { Id = 3, Title = "মেধাবী শিক্ষার্থীদের বৃত্তি প্রদান অনুষ্ঠান আগামীকাল", Category = "শিক্ষা", Icon = "🎓", PublishedAt = new DateTime(2026, 2, 22) },
                new News { Id = 4, Title = "কৃষকদের জন্য বিনামূল্যে সার ও বীজ বিতরণ কার্যক্রম", Category = "কৃষি", Icon = "🌾", PublishedAt = new DateTime(2026, 2, 18) },
                new News { Id = 5, Title = "বিনামূল্যে স্বাস্থ্য সেবা ক্যাম্প এই মাসে অনুষ্ঠিত হবে", Category = "স্বাস্থ্য", Icon = "🏥", PublishedAt = new DateTime(2026, 2, 15) },
                new News { Id = 6, Title = "মসজিদের সংস্কার ও সৌন্দর্যবর্ধন কাজ সম্পন্ন হয়েছে", Category = "ধর্মীয়", Icon = "🕌", PublishedAt = new DateTime(2026, 2, 10) }
            );

            // Seed Facilities
            modelBuilder.Entity<Facility>().HasData(
                new Facility { Id = 1, Name = "মসজিদ", Description = "গ্রামে ঐতিহাসিক জামে মসজিদ রয়েছে যেখানে প্রতিদিন পাঁচ ওয়াক্ত নামাজ আদায় হয়।", Icon = "🕌", Count = "৩টি মসজিদ", Category = "ধর্মীয়", SortOrder = 1 },
                new Facility { Id = 2, Name = "বিদ্যালয়", Description = "প্রাথমিক ও মাধ্যমিক বিদ্যালয় রয়েছে যেখানে শিশুরা মানসম্মত শিক্ষা গ্রহণ করতে পারে।", Icon = "🏫", Count = "২টি বিদ্যালয়", Category = "শিক্ষা", SortOrder = 2 },
                new Facility { Id = 3, Name = "মাদ্রাসা", Description = "ইসলামি শিক্ষার জন্য গ্রামে মাদ্রাসা রয়েছে যেখানে কুরআন ও হাদিস শিক্ষা দেওয়া হয়।", Icon = "📿", Count = "২টি মাদ্রাসা", Category = "শিক্ষা", SortOrder = 3 },
                new Facility { Id = 4, Name = "বাজার", Description = "সাপ্তাহিক হাট বাজার রয়েছে যেখানে নিত্যপ্রয়োজনীয় পণ্য কেনাবেচা হয়।", Icon = "🛒", Count = "১টি বাজার", Category = "ব্যবসা", SortOrder = 4 },
                new Facility { Id = 5, Name = "স্বাস্থ্যকেন্দ্র", Description = "কমিউনিটি ক্লিনিক রয়েছে যেখানে প্রাথমিক স্বাস্থ্যসেবা পাওয়া যায়।", Icon = "🏥", Count = "১টি ক্লিনিক", Category = "স্বাস্থ্য", SortOrder = 5 },
                new Facility { Id = 6, Name = "পাঠাগার", Description = "গ্রামের পাঠাগারে বই পড়ার সুযোগ রয়েছে। জ্ঞান অর্জনের কেন্দ্র হিসেবে কাজ করে।", Icon = "📚", Count = "১টি পাঠাগার", Category = "শিক্ষা", SortOrder = 6 }
            );

            // Seed Committee Members
            modelBuilder.Entity<CommitteeMember>().HasData(
                new CommitteeMember { Id = 1, Name = "মো. আব্দুল করিম", Role = "ইউনিয়ন পরিষদ সদস্য", Phone = "০১৭১২-৩৪৫৬৭৮", Avatar = "👨", SortOrder = 1 },
                new CommitteeMember { Id = 2, Name = "মো. রহিম উদ্দিন", Role = "গ্রাম প্রধান", Phone = "০১৮২৩-৪৫৬৭৮৯", Avatar = "👨‍🦳", SortOrder = 2 },
                new CommitteeMember { Id = 3, Name = "হাফেজ আলী হোসেন", Role = "মসজিদ ইমাম", Phone = "০১৯১১-২৩৪৫৬৭", Avatar = "🧔", SortOrder = 3 },
                new CommitteeMember { Id = 4, Name = "মোছা. সালেহা বেগম", Role = "মহিলা কমিটি সভাপতি", Phone = "০১৬৭৮-৯০১২৩৪", Avatar = "👩", SortOrder = 4 },
                new CommitteeMember { Id = 5, Name = "মো. কামরুল ইসলাম", Role = "বিদ্যালয় প্রধান শিক্ষক", Phone = "০১৫৫৫-৬৭৮৯০১", Avatar = "👨‍🏫", SortOrder = 5 },
                new CommitteeMember { Id = 6, Name = "ডা. নাজমুল হক", Role = "স্বাস্থ্য কমিটি প্রধান", Phone = "০১৭৩৩-৪৫৬৭৮৯", Avatar = "👨‍⚕️", SortOrder = 6 }
            );

            // Seed Gallery Items
            modelBuilder.Entity<GalleryItem>().HasData(
                new GalleryItem { Id = 1, Title = "ঐতিহাসিক জামে মসজিদ", Icon = "🕌", GradientColor = "linear-gradient(135deg, #2d9b5a, #1a6b3c)", IsFeatured = true, SortOrder = 1 },
                new GalleryItem { Id = 2, Title = "সোনালি ফসলের মাঠ", Icon = "🌾", GradientColor = "linear-gradient(135deg, #c8993a, #e8b84b)", SortOrder = 2 },
                new GalleryItem { Id = 3, Title = "প্রাথমিক বিদ্যালয়", Icon = "🏫", GradientColor = "linear-gradient(135deg, #1e5f3a, #2d9b5a)", SortOrder = 3 },
                new GalleryItem { Id = 4, Title = "গ্রামের পুকুর", Icon = "🌊", GradientColor = "linear-gradient(135deg, #0f4226, #1a6b3c)", SortOrder = 4 },
                new GalleryItem { Id = 5, Title = "সাপ্তাহিক হাট বাজার", Icon = "🛒", GradientColor = "linear-gradient(135deg, #b8891a, #c8993a)", SortOrder = 5 }
            );

            // Seed Ticker News
            modelBuilder.Entity<TickerNews>().HasData(
                new TickerNews { Id = 1, Text = "উজালপুর গ্রামে নতুন প্রাথমিক বিদ্যালয় ভবন নির্মাণ কাজ শুরু হয়েছে", SortOrder = 1 },
                new TickerNews { Id = 2, Text = "গ্রামের সড়ক উন্নয়নে ৫০ লক্ষ টাকা বরাদ্দ অনুমোদন পেয়েছে", SortOrder = 2 },
                new TickerNews { Id = 3, Text = "ঈদুল ফিতর উপলক্ষে জামে মসজিদে বিশেষ আলোচনা সভা আয়োজিত হবে", SortOrder = 3 },
                new TickerNews { Id = 4, Text = "মেধাবী শিক্ষার্থীদের বৃত্তি প্রদান অনুষ্ঠান আগামী সপ্তাহে অনুষ্ঠিত হবে", SortOrder = 4 },
                new TickerNews { Id = 5, Text = "কৃষকদের জন্য বিনামূল্যে কৃষি প্রশিক্ষণ শিবির আয়োজন করা হয়েছে", SortOrder = 5 }
            );
        }
    }
}
