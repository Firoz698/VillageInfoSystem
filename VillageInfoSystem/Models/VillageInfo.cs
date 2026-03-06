using System.ComponentModel.DataAnnotations;

namespace VillageInfoSystem.Models;

public class VillageInfo
{
    public int Id { get; set; }

    [Display(Name = "গ্রামের নাম")]
    public string VillageName { get; set; } = "";

    [Display(Name = "বিভাগ")]
    public string Division { get; set; } = "";

    [Display(Name = "গ্রামের ইতিহাস")]
    public string? History { get; set; }

    [Display(Name = "মোট জনসংখ্যা")]
    public string TotalPopulation { get; set; } = "";

    [Display(Name = "মোট পরিবার")]
    public string TotalFamilies { get; set; } = "";

    [Display(Name = "আয়তন")]
    public string Area { get; set; } = "";

    [Display(Name = "পুরুষ জনসংখ্যা")]
    public string MalePopulation { get; set; } = "";

    [Display(Name = "মহিলা জনসংখ্যা")]
    public string FemalePopulation { get; set; } = "";

    [Display(Name = "সাক্ষরতার হার")]
    public string LiteracyRate { get; set; } = "";

    [Display(Name = "প্রতিষ্ঠার সাল")]
    public string FoundedYear { get; set; } = "";

    [Display(Name = "যোগাযোগ ঠিকানা")]
    public string? Address { get; set; }

    [Display(Name = "ফোন")]
    public string? Phone { get; set; }

    [Display(Name = "ইমেইল")]
    public string? Email { get; set; }

    [Display(Name = "ফেসবুক পেজ")]
    public string? Facebook { get; set; }

    [Display(Name = "Google Maps লিংক")]
    public string? MapsLink { get; set; }

    [Display(Name = "যোগাযোগের সময়")]
    public string? OfficeHours { get; set; }
}

public class GalleryItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "শিরোনাম আবশ্যক")]
    [Display(Name = "শিরোনাম")]
    public string Title { get; set; } = "";

    [Display(Name = "আইকন (Emoji)")]
    public string Icon { get; set; } = "🖼️";

    [Display(Name = "রং (CSS gradient)")]
    public string GradientColor { get; set; } = "linear-gradient(135deg, #2d9b5a, #1a6b3c)";

    [Display(Name = "ইমেজ পাথ (ঐচ্ছিক)")]
    public string? ImagePath { get; set; }

    [Display(Name = "ফিচার্ড (বড় ছবি)")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "সক্রিয়")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "ক্রম")]
    public int SortOrder { get; set; } = 0;
}

public class TickerNews
{
    public int Id { get; set; }

    [Required(ErrorMessage = "টেক্সট আবশ্যক")]
    [Display(Name = "টিকার টেক্সট")]
    public string Text { get; set; } = "";

    [Display(Name = "সক্রিয়")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "ক্রম")]
    public int SortOrder { get; set; } = 0;
}
