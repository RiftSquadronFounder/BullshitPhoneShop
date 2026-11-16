using System.Windows;

namespace BullshitPhoneShop
{
    public partial class EditWindow : Window
    {
        private readonly int phoneId;

        public EditWindow(Phone phone)
        {
            InitializeComponent();
            phoneId = phone.Id;
            phoneForm.LoadFields(phone);
        }

        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            Phone? phone = phoneForm.ReadPhone(phoneId);
            if (phone is not null && App.TryDatabaseAction(this, () => DatabaseControl.UpdatePhone(phone)))
                DialogResult = true;
        }
    }
}
