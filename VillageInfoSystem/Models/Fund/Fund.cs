using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillageInfoSystem.Models.Fund
{
    public class Fund
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Member")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Receipt")]
        public string? PaymentImage { get; set; }

        [StringLength(500)]
        [Display(Name = "Payment Note")]
        public string? PaymentNote { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
