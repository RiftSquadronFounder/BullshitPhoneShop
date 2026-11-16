namespace BullshitPhoneShop
{
    public class Phone
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public decimal Price { get; set; }
        public Company CompanyEntity { get; set; } = null!;
    }
}
