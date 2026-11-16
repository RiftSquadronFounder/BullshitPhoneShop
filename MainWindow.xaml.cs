using System.Windows;

namespace BullshitPhoneShop
{
    public partial class MainWindow : Window
    {
        private readonly List<Phone> phones;
        private Phone? editingPhone;

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
            if (!TryReadPhoneFields(out string title, out string company, out decimal price))
                return;

            phones.Add(new Phone { Title = title, Company = company, Price = price });
            RefreshList();
            ClearPhoneFields();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem is not Phone phone)
                return;

            editingPhone = phone;
            titleView.Text = phone.Title;
            companyView.Text = phone.Company;
            priceView.Text = phone.Price.ToString();

            mainListBox.IsEnabled = false;
            phoneButtons.IsEnabled = false;
            editActions.Visibility = Visibility.Visible;
            titleView.Focus();
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e) => EndEditing();

        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (editingPhone is null || !TryReadPhoneFields(out string title, out string company, out decimal price))
                return;

            editingPhone.Title = title;
            editingPhone.Company = company;
            editingPhone.Price = price;
            RefreshList();
            EndEditing();
        }

        private void EndEditing()
        {
            editingPhone = null;
            ClearPhoneFields();
            mainListBox.IsEnabled = true;
            phoneButtons.IsEnabled = true;
            editActions.Visibility = Visibility.Collapsed;
            mainListBox.Focus();
        }

        private bool TryReadPhoneFields(out string title, out string company, out decimal price)
        {
            title = titleView.Text.Trim();
            company = companyView.Text.Trim();
            price = 0;

            if (title.Length == 0 || company.Length == 0)
            {
                MessageBox.Show(this, "Укажите модель и производителя.", Title);
                return false;
            }

            if (!decimal.TryParse(priceView.Text, out price) || price < 0)
            {
                MessageBox.Show(this, "Укажите корректную цену.", Title);
                return false;
            }

            return true;
        }

        private void ClearPhoneFields()
        {
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
