using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BullshitPhoneShop
{
    public static class DatabaseControl
    {
        public static void InitializeDatabase()
        {
            using var context = new DbAppContext();
            if (context.Database.EnsureCreated())
                return;

            using var transaction = context.Database.BeginTransaction();
            context.Database.ExecuteSqlRaw(
                "ALTER TABLE \"Phones\" ADD COLUMN IF NOT EXISTS \"Definition\" text NOT NULL DEFAULT '', " +
                $"ADD COLUMN IF NOT EXISTS \"Image\" text NOT NULL DEFAULT '{Phone.DefaultImage}'");

            var samples = DbAppContext.SamplePhones.ToDictionary(phone => phone.Id);
            int[] sampleIds = samples.Keys.ToArray();
            foreach (Phone phone in context.Phones.Where(phone => sampleIds.Contains(phone.Id)))
            {
                Phone sample = samples[phone.Id];
                if (phone.Title != sample.Title || phone.CompanyId != sample.CompanyId)
                    continue;

                if (string.IsNullOrWhiteSpace(phone.Definition))
                    phone.Definition = sample.Definition;

                if (string.IsNullOrWhiteSpace(phone.Image) || phone.Image == Phone.DefaultImage
                    || (Path.GetFileName(phone.Image) == Path.GetFileName(sample.Image) && !File.Exists(phone.Image)))
                    phone.Image = sample.Image;
            }

            context.SaveChanges();
            transaction.Commit();
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

        public static void AddPhone(Phone phone, string? imageSource = null)
        {
            using var context = new DbAppContext();
            context.Phones.Add(phone);
            SavePhone(context, phone, imageSource);
        }

        public static void UpdatePhone(Phone phone, string? imageSource = null)
        {
            using var context = new DbAppContext();
            Phone? storedPhone = context.Phones.FirstOrDefault(item => item.Id == phone.Id);
            if (storedPhone is null)
                return;

            storedPhone.Title = phone.Title;
            storedPhone.CompanyId = phone.CompanyId;
            storedPhone.Price = phone.Price;
            storedPhone.Definition = phone.Definition;
            string previousImage = storedPhone.Image;
            SavePhone(context, storedPhone, imageSource);

            if (imageSource is not null)
                DeleteUnusedImage(previousImage);
        }

        public static void RemovePhone(int id)
        {
            using var context = new DbAppContext();
            Phone? phone = context.Phones.FirstOrDefault(item => item.Id == id);
            if (phone is null)
                return;

            context.Phones.Remove(phone);
            context.SaveChanges();
            DeleteUnusedImage(phone.Image);
        }

        private static void SavePhone(DbAppContext context, Phone phone, string? imageSource)
        {
            string? uploadedImage = imageSource is null ? null : PhoneImages.Import(imageSource);
            if (uploadedImage is not null)
                phone.Image = uploadedImage;

            try
            {
                context.SaveChanges();
            }
            catch
            {
                DeleteUnusedImage(uploadedImage);
                throw;
            }
        }

        private static void DeleteUnusedImage(string? path)
        {
            if (path is null || !PhoneImages.Owns(path))
                return;

            try
            {
                using var context = new DbAppContext();
                if (!context.Phones.Any(phone => phone.Image == path))
                    PhoneImages.Delete(path);
            }
            catch (Exception error) when (error is NpgsqlException or TimeoutException
                or ArgumentException or IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning("Could not check whether the phone image is still used: {0}", error.Message);
            }
        }
    }
}
