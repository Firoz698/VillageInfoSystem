using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models.Fund
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "নাম আবশ্যক")]
        [StringLength(100)]
        [Display(Name = "পূর্ণ নাম")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "ফোন নম্বর আবশ্যক")]
        [StringLength(20)]
        [Display(Name = "ফোন নম্বর")]
        public string Phone { get; set; } = "";

        [StringLength(250)]
        [Display(Name = "ঠিকানা")]
        public string? Address { get; set; }

        [Display(Name = "প্রোফাইল ছবি")]
        public string? ProfileImage { get; set; }

        [Display(Name = "যোগদানের তারিখ")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Today;

        public ICollection<Fund> Funds { get; set; } = new List<Fund>();
    }
}