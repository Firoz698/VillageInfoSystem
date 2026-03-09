using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models
{
    public class DonationApplication
    {
        public int Id { get; set; }

        public int FundCampaignId { get; set; }

        [Required, Display(Name = "পূর্ণ নাম")]
        public string FullName { get; set; } = "";

        [Required, Display(Name = "মোবাইল নম্বর")]
        public string Phone { get; set; } = "";

        [Display(Name = "ইমেইল")]
        public string? Email { get; set; }

        [Required, Display(Name = "ঠিকানা")]
        public string Address { get; set; } = "";

        [Required, Display(Name = "দানের পরিমাণ (টাকা)")]
        public decimal Amount { get; set; }

        [Display(Name = "পেমেন্ট পদ্ধতি")]
        public string PaymentMethod { get; set; } = "bKash";

        [Display(Name = "ট্রানজেকশন আইডি")]
        public string? TransactionId { get; set; }

        [Display(Name = "বার্তা")]
        public string? Message { get; set; }

        // pending | accepted | rejected
        [Display(Name = "স্ট্যাটাস")]
        public string Status { get; set; } = "pending";

        [Display(Name = "আবেদনের তারিখ")]
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        [Display(Name = "প্রক্রিয়াকরণের তারিখ")]
        public DateTime? ProcessedAt { get; set; }

        [Display(Name = "Admin নোট")]
        public string? AdminNote { get; set; }

        // Navigation
        public FundCampaign? FundCampaign { get; set; }
    }
}
