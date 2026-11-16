using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BullshitPhoneShop
{
    public partial class MainWindow : Window
    {
        private Phone? editingPhone;

        public MainWindow()
        {
            InitializeComponent();

            phoneButtons.IsEnabled = TryDatabaseAction(() =>
            {
                DatabaseControl.InitializeDatabase();
                companyView.ItemsSource = DatabaseControl.GetCompanies();
                RefreshList();
            });
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryReadPhoneFields(out string title, out int companyId, out decimal price))
                return;

            var phone = new Phone { Title = title, CompanyId = companyId, Price = price };
            if (TryDatabaseAction(() => DatabaseControl.AddPhone(phone)))
            {
                ClearPhoneFields();
                TryDatabaseAction(RefreshList);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem is not Phone phone)
                return;

            editingPhone = phone;
            titleView.Text = phone.Title;
            companyView.SelectedValue = phone.CompanyId;
            priceView.Text = phone.Price.ToString();

            mainListBox.IsEnabled = false;
            phoneButtons.IsEnabled = false;
            editActions.Visibility = Visibility.Visible;
            titleView.Focus();
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e) => EndEditing();

        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (editingPhone is null || !TryReadPhoneFields(out string title, out int companyId, out decimal price))
                return;

            var phone = new Phone
            {
                Id = editingPhone.Id,
                Title = title,
                CompanyId = companyId,
                Price = price
            };

            if (TryDatabaseAction(() => DatabaseControl.UpdatePhone(phone)))
            {
                EndEditing();
                TryDatabaseAction(RefreshList);
            }
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

        private bool TryReadPhoneFields(out string title, out int companyId, out decimal price)
        {
            title = titleView.Text.Trim();
            companyId = 0;
            price = 0;

            if (title.Length == 0 || companyView.SelectedItem is not Company company)
            {
                MessageBox.Show(this, "Укажите модель и выберите производителя.", Title);
                return false;
            }

            if (!decimal.TryParse(priceView.Text, out price) || price < 0)
            {
                MessageBox.Show(this, "Укажите корректную цену.", Title);
                return false;
            }

            companyId = company.Id;
            return true;
        }

        private void ClearPhoneFields()
        {
            titleView.Clear();
            companyView.SelectedIndex = -1;
            priceView.Clear();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainListBox.SelectedItem is Phone phone &&
                TryDatabaseAction(() => DatabaseControl.RemovePhone(phone.Id)))
                TryDatabaseAction(RefreshList);
        }

        private void RefreshList()
        {
            mainListBox.ItemsSource = DatabaseControl.GetPhonesForView();
        }

        private bool TryDatabaseAction(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception error) when (error is NpgsqlException or DbUpdateException or TimeoutException
                or ArgumentException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show("Не удалось выполнить запрос к PostgreSQL. Проверьте сервер и параметры подключения в .env.",
                    Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
