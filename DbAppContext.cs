using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BullshitPhoneShop
{
    public class DbAppContext : DbContext
    {
        private const string ConnectionStringKey = "PHONE_SHOP_CONNECTION_STRING";

        internal static IReadOnlyList<Phone> SamplePhones { get; } = new[]
        {
            new Phone
            {
                Id = 1, Title = "iPhone 10", CompanyId = 1, Price = 58000,
                Definition = "iPhone X с OLED-экраном, системой распознавания лица Face ID и двойной камерой.",
                Image = SampleImage("iphone-x.jpg")
            },
            new Phone
            {
                Id = 2, Title = "Redmi Note 10S", CompanyId = 2, Price = 28000,
                Definition = "Смартфон с AMOLED-экраном, основной камерой на 64 Мп и аккумулятором на 5000 мА·ч.",
                Image = SampleImage("redmi-note-10s.jpg")
            },
            new Phone
            {
                Id = 3, Title = "iPhone 12 Pro Max Super Idol", CompanyId = 1, Price = 158000,
                Definition = "iPhone 12 Pro Max с 6,7-дюймовым OLED-экраном, поддержкой 5G и тройной камерой.",
                Image = SampleImage("iphone-12-pro-max.jpg")
            },
            new Phone
            {
                Id = 4, Title = "3310", CompanyId = 3, Price = 800,
                Definition = "Кнопочный телефон Nokia с монохромным экраном и игрой «Змейка».",
                Image = SampleImage("nokia-3310.jpg")
            }
        };

        public DbSet<Phone> Phones => Set<Phone>();
        public DbSet<Company> Company => Set<Company>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(GetConnectionString());

        internal void EnsurePhoneDetailsColumns()
        {
            try
            {
                AddMissingPhoneDetailsColumns();
            }
            catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.DuplicateColumn)
            {
                AddMissingPhoneDetailsColumns();
            }
        }

        private void AddMissingPhoneDetailsColumns()
        {
            var phone = Model.FindEntityType(typeof(Phone));
            var migration = new MigrationBuilder(Database.ProviderName);

            AddIfMissing(nameof(Phone.Definition), Phones.Select(item => item.Definition));
            AddIfMissing(nameof(Phone.Image), Phones.Select(item => item.Image));
            if (migration.Operations.Count == 0)
                return;

            var commands = this.GetService<IMigrationsSqlGenerator>().Generate(migration.Operations, Model);
            this.GetService<IMigrationCommandExecutor>().ExecuteNonQuery(commands, this.GetService<IRelationalConnection>());

            void AddIfMissing(string propertyName, IQueryable<string> column)
            {
                try
                {
                    column.Take(1).ToList();
                }
                catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.UndefinedColumn)
                {
                    var property = phone.FindProperty(propertyName);
                    migration.AddColumn<string>(
                        name: property.GetColumnName(),
                        table: phone.GetTableName(),
                        schema: phone.GetSchema(),
                        type: property.GetColumnType(),
                        nullable: property.IsNullable,
                        defaultValue: property.GetDefaultValue());
                }
            }
        }

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
            phones.Property(phone => phone.Definition).HasDefaultValue(string.Empty).IsRequired();
            phones.Property(phone => phone.Image).HasDefaultValue(Phone.DefaultImage).IsRequired()
                .IsConcurrencyToken();
            phones.HasCheckConstraint("CK_Phones_Price", "\"Price\" >= 0");
            phones.HasOne(phone => phone.CompanyEntity)
                .WithMany(company => company.PhoneEntities)
                .HasForeignKey(phone => phone.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            phones.HasData(SamplePhones);
        }

        private static string SampleImage(string fileName)
            => Path.Combine(AppContext.BaseDirectory, "Images", "Phones", fileName);

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
