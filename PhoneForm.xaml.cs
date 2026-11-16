using System.Windows;
using System.Windows.Controls;

namespace BullshitPhoneShop
{
    public partial class PhoneForm : UserControl
    {
        public PhoneForm()
        {
            InitializeComponent();
        }

        internal void LoadFields(Phone? phone = null)
        {
            companyView.ItemsSource = DatabaseControl.GetCompanies();
            if (phone is null)
                return;

            titleView.Text = phone.Title;
            companyView.SelectedValue = phone.CompanyId;
            priceView.Text = phone.Price.ToString();
        }

        internal Phone? ReadPhone(int id = 0)
        {
            string title = titleView.Text.Trim();
            if (title.Length == 0 || companyView.SelectedValue is not int companyId)
            {
                ShowValidationMessage("Укажите название и выберите компанию.");
                return null;
            }

            if (!decimal.TryParse(priceView.Text, out decimal price) || price < 0)
            {
                ShowValidationMessage("Введите число не меньше нуля в поле цены.");
                return null;
            }

            validationMessage.Visibility = Visibility.Collapsed;
            return new Phone { Id = id, Title = title, CompanyId = companyId, Price = price };
        }

        private void ShowValidationMessage(string message)
        {
            validationMessage.Text = message;
            validationMessage.Visibility = Visibility.Visible;
        }
    }
}
