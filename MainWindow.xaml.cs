using System.Windows;

namespace BullshitPhoneShop
{
    public partial class MainWindow : Window
    {
        private readonly List<Phone> phones;

        public MainWindow()
        {
            InitializeComponent();

            phones = new List<Phone>
            {
                new Phone { Title = "iPhone 10", Company = "Apple", Price = 58000 },
                new Phone { Title = "Redmi Note 10S", Company = "Xiaomi", Price = 28000 },
                new Phone { Title = "iPhone 12 Pro Max Super Idol", Company = "Apple", Price = 158000 },
                new Phone { Title = "3310", Company = "Nokia", Price = 800 }
            };

            mainListBox.ItemsSource = phones;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string title = titleView.Text.Trim();
            string company = companyView.Text.Trim();

            if (title.Length == 0 || company.Length == 0)
            {
                MessageBox.Show(this, "Укажите модель и производителя.", Title);
                return;
            }

            if (!decimal.TryParse(priceView.Text, out decimal price) || price < 0)
            {
                MessageBox.Show(this, "Укажите корректную цену.", Title);
                return;
            }

            phones.Add(new Phone { Title = title, Company = company, Price = price });
            RefreshList();

            titleView.Clear();
            companyView.Clear();
            priceView.Clear();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem is Phone phone)
            {
                phones.Remove(phone);
                RefreshList();
            }
        }

        private void RefreshList()
        {
            mainListBox.ItemsSource = null;
            mainListBox.ItemsSource = phones;
        }
    }
}
