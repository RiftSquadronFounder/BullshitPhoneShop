using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BullshitPhoneShop
{
    public partial class App : Application
    {
        internal static bool TryDatabaseAction(Window owner, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (PhoneImageException error)
            {
                MessageBox.Show(owner, error.Message, owner.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception error) when (error is NpgsqlException or DbUpdateException or TimeoutException
                or ArgumentException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(owner,
                    "Не удалось выполнить запрос к PostgreSQL. Проверьте сервер и параметры подключения в .env.",
                    owner.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
