using System.Windows;

namespace BullshitPhoneShop
{
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
            phoneForm.LoadFields();
        }

        private void AddPhoneButton_Click(object sender, RoutedEventArgs e)
        {
            Phone? phone = phoneForm.ReadPhone();
            if (phone is not null && App.TryDatabaseAction(this,
                () => DatabaseControl.AddPhone(phone, phoneForm.SelectedImagePath)))
                DialogResult = true;
        }
    }
}
