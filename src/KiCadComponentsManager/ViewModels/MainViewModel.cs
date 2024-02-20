using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using KiCadComponentsManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace KiCadComponentsManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    string connectionString = "DRIVER=SQLite3 ODBC Driver;Database=..\\..\\..\\..\\..\\test\\components.db;LongNames=0;Timeout=1000;NoTXN=0;SyncPragma=NORMAL;StepAPI=0;";

    public ObservableCollection<Category> Categories { get; } = [];

    public ObservableCollection<Dictionary<string, string>> _parts { get; } = [];

    public FlatTreeDataGridSource<Dictionary<string, string>> PartsSource { get; }

    [ObservableProperty]
    Dictionary<string, string>? selectedPart;

    public MainViewModel()
    {
        PartsSource = new FlatTreeDataGridSource<Dictionary<string, string>>(_parts);
        PartsSource.RowSelection.SelectionChanged += PartSelected;

        using (OdbcConnection conn = new(connectionString))
        {
            conn.Open();
            OdbcCommand command = new("SELECT name FROM sqlite_master WHERE type='table';", conn); 
            var tables = command.ExecuteReader();
            while (tables.Read())
            {
                string table = tables.GetString(0);
                Categories.Add(new(table));
            }
        }
    }

    private void PartSelected(object? sender, TreeSelectionModelSelectionChangedEventArgs<Dictionary<string, string>> e)
    {
        SelectedPart = e.SelectedItems[0];
    }

    private Category currentCategory;
    public Category CurrentCategory
    {
        get => currentCategory;
        set
        {
            SetProperty(ref currentCategory, value);

            OdbcConnection conn = new(connectionString);
            conn.Open();

            OdbcCommand command = new($"SELECT * FROM '{value}';", conn);
            var reader = command.ExecuteReader();
                

            List<string> columns = [];
            DataTable? schemaTable = reader.GetSchemaTable();

            PartsSource.Columns.Clear();
            List<TextColumn<Dictionary<string, string>, string>> cols = [];

            if (schemaTable != null) {
                foreach (DataRow row in schemaTable.Rows)
                {
                    if (row != null)
                    {
                        string? columnName = row["ColumnName"].ToString();
                        if (columnName != null){
                            columns.Add(columnName);
                            cols.Add(new TextColumn<Dictionary<string, string>, string>(columnName, x => x[columnName]));
                        }
                    }
                }
            }
            PartsSource.Columns.AddRange(cols);

            // Set the parts
            _parts.Clear();
            List<Dictionary<string, string>> parts = [];
            while (reader.Read())
            {
                Dictionary<string, string> part = [];
                foreach (string column in columns) {
                    var val = reader[column].ToString();
                    if (val != null) part[column] = val;
                }
                parts.Add(part);
            }
            _parts.AddRange(parts);
        }
    }
}
