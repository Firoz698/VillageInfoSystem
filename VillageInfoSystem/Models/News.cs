using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models;

public class News
{
    public int Id { get; set; }

    [Required(ErrorMessage = "শিরোনাম আবশ্যক")]
    [Display(Name = "শিরোনাম")]
    public string Title { get; set; } = "";

    [Display(Name = "বিবরণ")]
    public string? Description { get; set; }

    [Display(Name = "বিভাগ")]
    public string Category { get; set; } = "সাধারণ";

    [Display(Name = "আইকন (Emoji)")]
    public string Icon { get; set; } = "📰";

    [Display(Name = "প্রকাশের তারিখ")]
    public DateTime PublishedAt { get; set; } = DateTime.Now;

    [Display(Name = "শীর্ষ সংবাদ")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "সক্রিয়")]
    public bool IsActive { get; set; } = true;
}
