using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BullshitPhoneShop
{
    public partial class PhoneForm : UserControl
    {
        internal string? SelectedImagePath { get; private set; }

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
            definitionView.Text = phone.Definition;
            if (!string.IsNullOrWhiteSpace(phone.Image) && phone.Image != Phone.DefaultImage)
                selectedImageName.Text = "Изображение сохранено";
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
            return new Phone
            {
                Id = id, Title = title, CompanyId = companyId, Price = price,
                Definition = definitionView.Text.Trim()
            };
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filter = "Изображения JPEG и PNG (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                SelectedImagePath = dialog.FileName;
                selectedImageName.Text = Path.GetFileName(dialog.FileName);
            }
        }

        private void ShowValidationMessage(string message)
        {
            validationMessage.Text = message;
            validationMessage.Visibility = Visibility.Visible;
        }
    }
}
