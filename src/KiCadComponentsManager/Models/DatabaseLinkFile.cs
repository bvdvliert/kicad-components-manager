using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCadComponentsManager.Models
{
    public class AutoDefault
    {
        public string? key { get; set; }
        public string? value { get; set; }
        public List<List<string>>? defaults { get; set; }
    }

    public class Field
    {
        public string? column { get; set; }
        public string? name { get; set; }
        public string? digikey_property { get; set; }
        public bool visible_in_chooser { get; set; } = false;
        public bool visible_on_add { get; set; } = false;
        public bool is_link { get; set; } = false;
        public bool is_MPN { get; set; } = false;

    }

    public class Library
    {
        public string? name { get; set; }
        public string? table { get; set; }
        public string? key { get; set; }
        public string? symbols { get; set; }
        public string? footprints { get; set; }
        public string? digikey_category { get; set; }
        public List<Field>? fields { get; set; }
        public Properties? properties { get; set; }
    }

    public class Meta
    {
        public int version { get; set; }
    }

    public class Properties
    {
        public string? description { get; set; }
        public string? footprint_filters { get; set; }
        public string? keywords { get; set; }
        public string? exclude_from_bom { get; set; }
        public string? exclude_from_board { get; set; }
        public List<AutoDefault>? auto_default { get; set; }
    }

    public class DatabaseLinkFile
    {
        public Meta? meta { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public Source? source { get; set; }
        public List<Library>? libraries { get; set; }
    }

    public class Source
    {
        public string? type { get; set; }
        public int timeout_seconds { get; set; }
        public string? connection_string { get; set; }
    }
}
