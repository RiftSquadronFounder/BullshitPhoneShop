using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BullshitPhoneShop
{
    public partial class AboutPage : Page
    {
        public AboutPage(Phone phone)
        {
            InitializeComponent();
            titlePageView.Text = phone.Title;
            companyPageView.Text = phone.CompanyEntity.Title;
            pricePageView.Text = phone.Price.ToString();
            definitionPageView.Text = string.IsNullOrWhiteSpace(phone.Definition)
                ? "Описание отсутствует."
                : phone.Definition;
            imagePageView.Source = LoadPhoto(phone.Image)
                ?? new BitmapImage(new Uri(Phone.DefaultImage, UriKind.Absolute));
            Loaded += (_, _) => backButton.Focus();
        }

        private static BitmapImage? LoadPhoto(string path)
        {
            if (!Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) || !uri.IsFile || !File.Exists(uri.LocalPath))
                return null;

            try
            {
                return PhoneImages.Load(uri.LocalPath);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException
                or NotSupportedException or FileFormatException)
            {
                return null;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => FrameContext.MainWindowFrame?.Navigate((object)null!);
    }
}
