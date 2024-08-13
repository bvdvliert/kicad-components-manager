using KiCadComponentsManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KiCadComponentsManager.Services
{
    public class SettingsManager
    {
        public static string Location => LocationInfo.FullName;
        private static FileInfo LocationInfo => new(Path.Combine(DirectoryProvider.ApplicationData, "settings.json"));

        private static AppSettings? settings;
        public static AppSettings? Settings
        {
            get {
                return settings; 
            }
        }

        public static async Task Asyncinit()
        {
            settings ??= await GetAppSettings();
        }

        public static async Task<AppSettings> GetAppSettings()
        {
            var appSettings = await Read<AppSettings>(LocationInfo).ConfigureAwait(false);

            return appSettings;
        }

        public async Task UpdateAppSettings(AppSettings appSettings)
        {
            await Write(LocationInfo, appSettings).ConfigureAwait(false);
        }

        private static async Task<T> Read<T>(FileInfo file) where T : new()
        {
            if (!file.Exists)
            {
                return new T();
            }

            var json = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)!;
        }

        private static async Task Write<T>(FileInfo file, T value)
        {
            file.Directory!.Create();
            string json = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            await File.WriteAllTextAsync(file.FullName, json).ConfigureAwait(false);
        }

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
        };
    }
}
