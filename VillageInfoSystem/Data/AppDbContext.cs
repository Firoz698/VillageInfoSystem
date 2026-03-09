using Microsoft.EntityFrameworkCore;
using VillageInfoSystem.Models;
using VillageInfoSystem.Models.Fund;

namespace VillageInfoSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<News> News => Set<News>();
        public DbSet<SliderItem> SliderItems => Set<SliderItem>();
        public DbSet<Facility> Facilities => Set<Facility>();
        public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
        public DbSet<VillageInfo> VillageInfos => Set<VillageInfo>();
        public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();
        public DbSet<TickerNews> TickerNews => Set<TickerNews>();
        public DbSet<Institution> Institutions => Set<Institution>();
        public DbSet<InstitutionMember> InstitutionMembers => Set<InstitutionMember>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Fund> Funds => Set<Fund>();
        public DbSet<Donation> Donations => Set<Donation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InstitutionMember>()
                .HasOne(m => m.Institution)
                .WithMany(i => i.Members)
                .HasForeignKey(m => m.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}