using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models;

public class Facility
{
    public int Id { get; set; }

    [Required(ErrorMessage = "নাম আবশ্যক")]
    [Display(Name = "নাম")]
    public string Name { get; set; } = "";

    [Display(Name = "বিবরণ")]
    public string? Description { get; set; }

    [Display(Name = "আইকন (Emoji)")]
    public string Icon { get; set; } = "🏛️";

    [Display(Name = "সংখ্যা (যেমন: ৩টি মসজিদ)")]
    public string Count { get; set; } = "";

    [Display(Name = "বিভাগ")]
    public string Category { get; set; } = "সাধারণ";

    [Display(Name = "সক্রিয়")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "ক্রম")]
    public int SortOrder { get; set; } = 0;
}
