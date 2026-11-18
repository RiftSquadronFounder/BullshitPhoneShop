using BullshitPhoneShop.db;
using BullshitPhoneShop.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BullshitPhoneShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            FrameContext.MainWindowFrame = mainFrame;
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

        private void PhoneMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            var row = e.OriginalSource is DependencyObject source
                ? ItemsControl.ContainerFromElement(mainDataGridView, source) as DataGridRow
                : null;
            var phone = row?.Item as Phone;

            if (phone is null && e.CursorLeft < 0 && e.CursorTop < 0)
                phone = mainDataGridView.SelectedItem as Phone;

            if (phone is null)
            {
                e.Handled = true;
                return;
            }

            mainDataGridView.ContextMenu.DataContext = phone;
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if (e is MouseButtonEventArgs mouseEvent)
            {
                if (mouseEvent.ChangedButton != MouseButton.Left)
                    return;

                for (var visual = e.OriginalSource as Visual; visual is not null && visual != sender;
                     visual = VisualTreeHelper.GetParent(visual) as Visual)
                {
                    if (visual is Button)
                        return;
                }
            }

            if (sender is FrameworkElement { DataContext: Phone phone })
            {
                FrameContext.MainWindowFrame?.Navigate(new AboutPage(phone));
                e.Handled = true;
            }
        }

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

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            bool showingDetails = e.Content is AboutPage;
            mainFrame.Visibility = showingDetails ? Visibility.Visible : Visibility.Collapsed;
            phoneListPanel.Visibility = showingDetails ? Visibility.Collapsed : Visibility.Visible;

            if (!showingDetails)
            {
                mainFrame.RemoveBackEntry();
                mainDataGridView.Focus();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (FrameContext.MainWindowFrame == mainFrame)
                FrameContext.MainWindowFrame = null;

            base.OnClosed(e);
        }
    }
}
