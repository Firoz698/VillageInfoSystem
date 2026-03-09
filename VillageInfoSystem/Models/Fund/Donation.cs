using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillageInfoSystem.Models.Fund
{
    public class Donation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "শিরোনাম আবশ্যক")]
        [StringLength(200)]
        [Display(Name = "শিরোনাম")]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        [Display(Name = "বিবরণ")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "টাকার পরিমাণ আবশ্যক")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "পরিমাণ ০ এর বেশি হতে হবে")]
        [Display(Name = "পরিমাণ")]
        public decimal Amount { get; set; }

        [Display(Name = "ছবি")]
        public string? Image { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "তারিখ")]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}