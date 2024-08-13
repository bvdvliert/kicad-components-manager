using KiCadComponentsManager.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KiCadComponentsManager.Models
{
    public static class Workspace
    {
        public static DatabaseLinkFile? Dbl { get; private set; }

        static string? connectionString;

        public async static void Init()
        {
            await SettingsManager.Asyncinit();
            if (SettingsManager.Settings == null || SettingsManager.Settings.Workspace == null) return;
            StreamReader dblfile = new(SettingsManager.Settings.Workspace);
            Dbl = JsonConvert.DeserializeObject<DatabaseLinkFile>(dblfile.ReadToEnd());
            connectionString = Dbl.source.connection_string.Replace("${CWD}", SettingsManager.Settings.Workspace[..(SettingsManager.Settings.Workspace.LastIndexOf('/') + 1)]);
            if (string.IsNullOrEmpty(connectionString)) return;
            /*OdbcConnection conn = new(connectionString);
            conn.Open();
            OdbcCommand command = new("SELECT name FROM sqlite_master WHERE type='table';", conn);
            var tables = command.ExecuteReader();
            while (tables.Read())
            {
                string table = tables.GetString(0);
                //Categories.Add(new(table));
            }
            PartsSource.Columns.Clear();*/
            InitDone(null, EventArgs.Empty);
        }

        public static event EventHandler InitDone = delegate { };
    }
}
