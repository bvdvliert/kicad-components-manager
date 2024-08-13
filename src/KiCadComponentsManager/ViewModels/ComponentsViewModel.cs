using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using KiCadComponentsManager.Services;
using KiCadComponentsManager.Models;
using KiCadComponentsManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace KiCadComponentsManager.ViewModels;

public partial class ComponentsViewModel : ViewModelBase
{
    public ObservableCollection<Library> Libraries { get; private set; } = [];

    public ObservableCollection<Dictionary<string, string>> _parts { get; } = [];

    public FlatTreeDataGridSource<Dictionary<string, string>> PartsSource { get; }

    [ObservableProperty]
    Dictionary<string, string>? selectedPart;

    public ComponentsViewModel()
    {
        PartsSource = new FlatTreeDataGridSource<Dictionary<string, string>>(_parts);
        PartsSource.RowSelection.SelectionChanged += PartSelected;

        Workspace.InitDone += Workspace_InitDone;
        if (Workspace.Dbl == null) Workspace.Init();
    }

    private void Workspace_InitDone(object? sender, EventArgs e)
    {
        Libraries.Clear();
        Libraries.AddRange(Workspace.Dbl.libraries);
    }

    private void PartSelected(object? sender, TreeSelectionModelSelectionChangedEventArgs<Dictionary<string, string>> e)
    {
        SelectedPart = e.SelectedItems[0];
    }

    private Library currentLibrary;
    public Library CurrentLibrary
    {
        get => currentLibrary;
        set
        {
            SetProperty(ref currentLibrary, value);

            OdbcConnection conn = new(Workspace.Dbl.source.connection_string.Replace("${CWD}", SettingsManager.Settings.Workspace[..(SettingsManager.Settings.Workspace.LastIndexOf('/') + 1)]));
            conn.Open();

            OdbcCommand command = new($"SELECT * FROM '{currentLibrary.table}';", conn);
            var reader = command.ExecuteReader();
            
            List<TextColumn<Dictionary<string, string>, string>> cols = [];
            List<string> columns = [];
            foreach (Field field in currentLibrary.fields)
            {
                columns.Add(field.name);
                cols.Add(new(field.name, x => x[field.name]));
            }
            PartsSource.Columns.Clear();
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

    [ObservableProperty]
    ViewModelBase contentViewModel;

    [RelayCommand]
    public void AddComponent(Int32? x)
    {
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var store = new AddComponentViewModel();

        var dialog = new AddComponentWindow(store); // Your dialog window implementation

        store.RequestClose += (sender, args) => dialog.Close();

        var result = dialog.ShowDialog<AddComponentWindow>(mainWindow);

    }

    /// <summary>
    /// Gets or sets a list of Files
    /// </summary>
    [ObservableProperty]
    private IEnumerable<IStorageFile>? _SelectedFiles;

    /// <summary>
    /// A command used to select some files
    /// </summary>
    [RelayCommand]
    private async Task SelectFilesAsync()
    {
        SelectedFiles = await this.OpenFileDialogAsync();
        if (SelectedFiles == null) return;
        AppSettings settings = new();
        var file = SelectedFiles.First();
        settings.Workspace = file.Path.AbsolutePath;
        SettingsProvider provider = new();
        await provider.UpdateAppSettings(settings);

    }
}