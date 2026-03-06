using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models
{
    public class InstitutionMember
    {
        public int Id { get; set; }

        public int InstitutionId { get; set; }

        [Required, Display(Name = "নাম")]
        public string Name { get; set; } = "";

        [Display(Name = "পদবি / ভূমিকা")]
        public string Role { get; set; } = "";

        [Display(Name = "পেশা")]
        public string Profession { get; set; } = "";

        [Display(Name = "ফোন")]
        public string Phone { get; set; } = "";

        [Display(Name = "ছবি পাথ")]
        public string? ImagePath { get; set; }

        [Display(Name = "Avatar emoji")]
        public string Avatar { get; set; } = "👤";

        [Display(Name = "ক্রম")]
        public int SortOrder { get; set; } = 0;

        // Navigation
        public Institution? Institution { get; set; }
    }
}
