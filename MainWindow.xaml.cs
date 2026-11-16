using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace BullshitPhoneShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            addButton.IsEnabled = App.TryDatabaseAction(this, () =>
            {
                DatabaseControl.InitializeDatabase();
                RefreshTable();
            });
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowPhoneDialog(() => new AddWindow());

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: Phone phone })
                ShowPhoneDialog(() => new EditWindow(phone));
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: Phone phone })
            {
                App.TryDatabaseAction(this, () =>
                {
                    DatabaseControl.RemovePhone(phone.Id);
                    RefreshTable();
                });
            }
        }

        private void ShowPhoneDialog(Func<Window> createDialog)
        {
            App.TryDatabaseAction(this, () =>
            {
                Window dialog = createDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                    RefreshTable();
            });
        }

        private void RefreshTable()
        {
            mainDataGridView.ItemsSource = DatabaseControl.GetPhonesForView();
        }
    }
}
