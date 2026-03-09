using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillageInfoSystem.Models.Fund
{
    public class Fund
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "সদস্য")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "টাকার পরিমাণ আবশ্যক")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "পরিমাণ ০ এর বেশি হতে হবে")]
        [Display(Name = "পরিমাণ")]
        public decimal Amount { get; set; }

        [Display(Name = "পেমেন্ট রসিদ")]
        public string? PaymentImage { get; set; }

        [StringLength(500)]
        [Display(Name = "পেমেন্ট নোট")]
        public string? PaymentNote { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "তারিখ")]
        public DateTime Date { get; set; } = DateTime.Today;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}