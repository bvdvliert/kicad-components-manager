using System.Threading.Tasks;
using KiCadComponentsManager.Models;

namespace KiCadComponentsManager.Services
{
    public interface ISettingsProvider
    {
        string Location { get; }

        Task<AppSettings> GetAppSettings();

        Task UpdateAppSettings(AppSettings appSettings);
    }
}