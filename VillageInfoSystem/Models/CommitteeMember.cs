using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models;

public class CommitteeMember
{
    public int Id { get; set; }

    [Required(ErrorMessage = "নাম আবশ্যক")]
    [Display(Name = "নাম")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "পদবী আবশ্যক")]
    [Display(Name = "পদবী")]
    public string Role { get; set; } = "";

    [Display(Name = "ফোন নম্বর")]
    public string? Phone { get; set; }

    [Display(Name = "আইকন (Emoji)")]
    public string Avatar { get; set; } = "👤";

    [Display(Name = "বিস্তারিত")]
    public string? Bio { get; set; }

    [Display(Name = "সক্রিয়")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "ক্রম")]
    public int SortOrder { get; set; } = 0;
}
