﻿using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using KiCadComponentsManager.Models;
using KiCadComponentsManager.Services;

namespace KiCadComponentsManager.Services
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
        };

        public string Location => LocationInfo.FullName;

        private static FileInfo LocationInfo => new(Path.Combine(DirectoryProvider.ApplicationData, "settings.json"));

        public async Task<AppSettings> GetAppSettings()
        {
            var appSettings = await Read<AppSettings>(LocationInfo).ConfigureAwait(false);

            /*if (Directory.Exists(appSettings.Workspace))
            {
                Directory.SetCurrentDirectory(appSettings.Workspace);
            }
            else
            {
                appSettings.Workspace = null;
            }*/

            return appSettings;
        }

        public async Task UpdateAppSettings(AppSettings appSettings)
        {
            await Write(LocationInfo, appSettings).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(appSettings.Workspace))
            {
                //TODO (May be deleted?) Directory.SetCurrentDirectory(appSettings.Workspace);
            }
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

        private async Task<FileInfo> GetSettingsFileInfo()
        {
            var appSettigns = await GetAppSettings().ConfigureAwait(false);
            if (string.IsNullOrEmpty(appSettigns.Workspace))
            {
                throw new InvalidOperationException("Unknown workspace.");
            }

            var fileName = Path.Combine(appSettigns.Workspace, ".kicad-db-lib", "settings.json");
            return new FileInfo(fileName);
        }
    }
}