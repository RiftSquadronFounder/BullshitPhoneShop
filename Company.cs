namespace BullshitPhoneShop
{
    public class Company
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CEO { get; set; } = string.Empty;
        public double Capital { get; set; }
        public List<Phone> PhoneEntities { get; set; } = new List<Phone>();
    }
}
