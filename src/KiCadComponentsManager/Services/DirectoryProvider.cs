using System;
using System.IO;

namespace KiCadComponentsManager.Services
{
    public static class DirectoryProvider
    {
        public static string ApplicationData => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "kicad-components-manager");
    }
}