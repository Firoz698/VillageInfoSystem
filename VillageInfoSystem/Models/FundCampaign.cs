using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models
{
    public class FundCampaign
    {
        public int Id { get; set; }

        [Required, Display(Name = "ফান্ডের নাম")]
        public string Title { get; set; } = "";

        [Display(Name = "বিবরণ")]
        public string Description { get; set; } = "";

        [Display(Name = "আইকন (emoji)")]
        public string Icon { get; set; } = "💰";

        [Display(Name = "লক্ষ্যমাত্রা (টাকা)")]
        public decimal GoalAmount { get; set; } = 0;

        [Display(Name = "সংগৃহীত মোট (টাকা)")]
        public decimal CollectedAmount { get; set; } = 0;

        [Display(Name = "সক্রিয়")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "তৈরির তারিখ")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public List<DonationApplication> Applications { get; set; } = new();
    }
}
