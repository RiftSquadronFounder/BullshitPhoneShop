using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace BullshitPhoneShop
{
    internal static class PhoneImages
    {
        private static readonly string StorageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BullshitPhoneShop", "Images");

        internal static string Import(string source)
        {
            string extension = Path.GetExtension(source).ToLowerInvariant();
            if (extension is not (".jpg" or ".jpeg" or ".png"))
                throw new PhoneImageException();

            string destination = Path.Combine(StorageFolder, Guid.NewGuid().ToString("N") + extension);
            bool created = false;

            try
            {
                Directory.CreateDirectory(StorageFolder);
                using (FileStream input = File.OpenRead(source))
                using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
                {
                    created = true;
                    input.CopyTo(output);
                }

                Load(destination);
                return destination;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException
                or NotSupportedException or FileFormatException)
            {
                if (created)
                    Delete(destination);

                throw new PhoneImageException(error);
            }
        }

        internal static BitmapImage Load(string path)
        {
            using FileStream stream = File.OpenRead(path);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelHeight = 650;
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        internal static bool Owns(string? path)
        {
            return path is not null
                && string.Equals(Path.GetDirectoryName(path), StorageFolder, StringComparison.OrdinalIgnoreCase)
                && Guid.TryParseExact(Path.GetFileNameWithoutExtension(path), "N", out _);
        }

        internal static void Delete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning("Could not delete an unused phone image: {0}", error.Message);
            }
        }
    }

    internal sealed class PhoneImageException(Exception? innerException = null)
        : IOException("Не удалось загрузить изображение. Выберите доступный файл JPEG или PNG.", innerException);
}
