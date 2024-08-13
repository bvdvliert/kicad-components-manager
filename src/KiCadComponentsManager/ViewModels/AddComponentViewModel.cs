using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KiCadComponentsManager.Services;
using Avalonia.Collections;
using DynamicData;
using KiCadComponentsManager.Models;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using KiCadComponentsManager.Views;
using Microsoft.CodeAnalysis;
using System.Data;
using static KiCadComponentsManager.Services.DatabaseManager;

namespace KiCadComponentsManager.ViewModels
{
    public partial class AddComponentViewModel : ViewModelBase
    {
        public event EventHandler? RequestClose;

        public ObservableCollection<KeyValuePair<string, string>> Component { get; set; } = [];

        [ObservableProperty]
        string mPN = "";

        Library? lib;
        public Library? Lib
        {
            get { return lib; }
            set { 
                SetProperty(ref lib, value);
                Component.Clear();
                foreach (Field f in lib.fields)
                {
                    if(f.name != null) Component.Add(new(f.name, string.Empty));
                }
                if (lib.properties.description != null) Component.Add(new("Description", string.Empty));
                if (lib.symbols != null) Component.Add(new("Symbol", string.Empty));
                if (lib.footprints != null) Component.Add(new("Footprint", string.Empty));
            }
        }

        public ObservableCollection<Library> Libraries { get; private set; } = [];


        Dictionary<string,string> propertys = [];

        bool mPNFocused;
        public bool MPNFocused
        {
            get => mPNFocused;
            set { 
                mPNFocused = value;
                if (!value)
                    GetProperties();
            }
        }

        async void GetProperties()
        {
            if (string.IsNullOrEmpty(mPN)) return;
            propertys = await DigikeyClient.GetComponentInfo(mPN);

            if (string.IsNullOrEmpty(propertys["digikey_category"])) return;
            foreach(var l in Libraries)
            {
                if (l.digikey_category == propertys["digikey_category"])
                {
                    lib = l;
                    OnPropertyChanged(nameof(Lib));

                    Dictionary<string, string> componentProperties = [];
                    foreach (Field f in lib.fields)
                    {
                        if (!f.is_MPN)
                        {
                            propertys.TryGetValue(f.digikey_property, out string value);
                            if (f.name != null) componentProperties.Add(f.name, value);
                        }
                    }
                    if (lib.symbols != null) componentProperties.Add(lib.symbols, string.Empty);
                    if (lib.footprints != null) componentProperties.Add(lib.footprints, string.Empty);

                    foreach (AutoDefault autodef in l.properties.auto_default)
                    {
                        string keyval = "";
                        if(!string.IsNullOrEmpty(autodef.key))
                            keyval = componentProperties[autodef.key];

                        foreach (var def in autodef.defaults)
                        {
                            if (def[0] == keyval)
                            {
                                componentProperties[autodef.value] = def[1];
                                break;
                            }
                        }
                    }

                    Component.Clear();
                    Component.AddRange(componentProperties);

                    break;
                }
            }
        }
                
        public AddComponentViewModel() {
            Libraries.Add(Workspace.Dbl.libraries);

            DigikeyClient.Login();
        }

        [RelayCommand]
        public void Confirm()
        {
            string columns = string.Empty;
            string values = string.Empty;
            foreach (var col in Component)
            {
                if (!string.IsNullOrEmpty(columns) && string.IsNullOrEmpty(values))
                {
                    columns = col.Key;
                    values = col.Value;
                }
                else
                {
                    columns += $",\"{col.Key}\"";
                    values += $",\"{col.Value}\"";
                }
            }

            string sql = $"INSERT INTO \"main.\".\"{Lib.table}\"({columns}) VALUES ({values});";

            ExecuteSQLCommand(sql);

        }

        [RelayCommand]
        public void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);

            //destroy;
        }
    }
}
