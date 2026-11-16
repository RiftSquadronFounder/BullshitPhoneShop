using System.IO;
using Microsoft.EntityFrameworkCore;

namespace BullshitPhoneShop
{
    public class DbAppContext : DbContext
    {
        private const string ConnectionStringKey = "PHONE_SHOP_CONNECTION_STRING";

        public DbSet<Phone> Phones => Set<Phone>();
        public DbSet<Company> Company => Set<Company>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(GetConnectionString());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var companies = modelBuilder.Entity<Company>();
            companies.Property(company => company.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 5);
            companies.Property(company => company.Title).IsRequired();
            companies.Property(company => company.CEO).IsRequired();
            companies.HasIndex(company => company.Title).IsUnique();
            companies.HasData(
                new Company { Id = 1, Title = "Apple", CEO = "Tim Cook", Capital = 3000 },
                new Company { Id = 2, Title = "Xiaomi", CEO = "Lei Jun", Capital = 50 },
                new Company { Id = 3, Title = "Nokia", CEO = "Pekka Lundmark", Capital = 20 },
                new Company { Id = 4, Title = "Samsung", CEO = "Jong-Hee Han", Capital = 350 });

            var phones = modelBuilder.Entity<Phone>();
            phones.Property(phone => phone.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 5);
            phones.Property(phone => phone.Title).IsRequired();
            phones.HasCheckConstraint("CK_Phones_Price", "\"Price\" >= 0");
            phones.HasOne(phone => phone.CompanyEntity)
                .WithMany(company => company.PhoneEntities)
                .HasForeignKey(phone => phone.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            phones.HasData(
                new Phone { Id = 1, Title = "iPhone 10", CompanyId = 1, Price = 58000 },
                new Phone { Id = 2, Title = "Redmi Note 10S", CompanyId = 2, Price = 28000 },
                new Phone { Id = 3, Title = "iPhone 12 Pro Max Super Idol", CompanyId = 1, Price = 158000 },
                new Phone { Id = 4, Title = "3310", CompanyId = 3, Price = 800 });
        }

        private static string GetConnectionString()
        {
            string? configured = Environment.GetEnvironmentVariable(ConnectionStringKey);
            if (!string.IsNullOrWhiteSpace(configured))
                return configured;

            for (DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
                 directory is not null;
                 directory = directory.Parent)
            {
                string path = Path.Combine(directory.FullName, ".env");
                if (!File.Exists(path))
                    continue;

                foreach (string line in File.ReadLines(path))
                {
                    string[] parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2 || parts[0] != ConnectionStringKey || parts[1].Length == 0)
                        continue;

                    string value = parts[1];
                    return value.Length >= 2 && (value[0] is '\'' or '"') && value[0] == value[^1]
                        ? value[1..^1]
                        : value;
                }
            }

            throw new ArgumentException($"Set {ConnectionStringKey} in .env or in the environment.");
        }
    }
}
