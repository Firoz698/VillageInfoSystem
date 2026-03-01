namespace VillageInfoSystem.Models
{
    public class SliderItem
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }  
        public string Badge { get; set; }       
        public string Title { get; set; }   
        public string TitleHighlight { get; set; } 
        public string SubText { get; set; }
        public string Btn1Text { get; set; }
        public string Btn1Link { get; set; }
        public string Btn2Text { get; set; }
        public string Btn2Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
