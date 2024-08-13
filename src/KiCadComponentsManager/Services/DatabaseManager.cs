using KiCadComponentsManager.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCadComponentsManager.Services
{
    public static class DatabaseManager
    {
        public static OdbcDataReader ExecuteSQLCommand(string command)
        {
            OdbcConnection conn = new(Workspace.Dbl.source.connection_string.Replace("${CWD}", SettingsManager.Settings.Workspace[..(SettingsManager.Settings.Workspace.LastIndexOf('/') + 1)]));

            conn.Open();

            OdbcCommand cmd = new(command, conn);

            return cmd.ExecuteReader();
        }
    }
}
