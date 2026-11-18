namespace BullshitPhoneShop.Models
{
    public class Phone
    {
        public const string DefaultImage = "pack://application:,,,/BullshitPhoneShop;component/Images/no-image.png";

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public decimal Price { get; set; }
        public string Definition { get; set; } = string.Empty;
        public string Image { get; set; } = DefaultImage;
        public Company CompanyEntity { get; set; } = null!;
    }
}
