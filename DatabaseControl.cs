using Microsoft.EntityFrameworkCore;

namespace BullshitPhoneShop
{
    public static class DatabaseControl
    {
        public static void InitializeDatabase()
        {
            using var context = new DbAppContext();
            context.Database.EnsureCreated();
        }

        public static List<Phone> GetPhonesForView()
        {
            using var context = new DbAppContext();
            return context.Phones.AsNoTracking()
                .Include(phone => phone.CompanyEntity)
                .OrderBy(phone => phone.Id)
                .ToList();
        }

        public static List<Company> GetCompanies()
        {
            using var context = new DbAppContext();
            return context.Company.AsNoTracking()
                .OrderBy(company => company.Title)
                .ToList();
        }

        public static void AddPhone(Phone phone)
        {
            using var context = new DbAppContext();
            context.Phones.Add(phone);
            context.SaveChanges();
        }

        public static void UpdatePhone(Phone phone)
        {
            using var context = new DbAppContext();
            Phone? storedPhone = context.Phones.FirstOrDefault(item => item.Id == phone.Id);
            if (storedPhone is null)
                return;

            storedPhone.Title = phone.Title;
            storedPhone.CompanyId = phone.CompanyId;
            storedPhone.Price = phone.Price;
            context.SaveChanges();
        }

        public static void RemovePhone(int id)
        {
            using var context = new DbAppContext();
            Phone? phone = context.Phones.FirstOrDefault(item => item.Id == id);
            if (phone is null)
                return;

            context.Phones.Remove(phone);
            context.SaveChanges();
        }
    }
}
