using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models
{
    public class Institution
    {
        public int Id { get; set; }

        [Required, Display(Name = "প্রতিষ্ঠানের নাম")]
        public string Name { get; set; } = "";

        [Display(Name = "বিভাগ / ধরন")]
        public string Category { get; set; } = ""; // মসজিদ, স্কুল, ক্লাব ইত্যাদি

        [Display(Name = "সংক্ষিপ্ত বিবরণ")]
        public string Description { get; set; } = "";

        [Display(Name = "বিস্তারিত বিবরণ")]
        public string Details { get; set; } = "";

        [Display(Name = "প্রতিষ্ঠাকাল")]
        public string FoundedYear { get; set; } = "";

        [Display(Name = "প্রতিষ্ঠাতার নাম")]
        public string FounderName { get; set; } = "";

        [Display(Name = "ইমেজ পাথ")]
        public string? ImagePath { get; set; }

        [Display(Name = "আইকন (emoji)")]
        public string Icon { get; set; } = "🏛️";

        [Display(Name = "সক্রিয়")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ক্রম")]
        public int SortOrder { get; set; } = 0;

        // Navigation
        public List<InstitutionMember> Members { get; set; } = new();
    }
}
