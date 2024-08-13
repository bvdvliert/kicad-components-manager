using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KiCadComponentsManager.Secrets;
using Newtonsoft.Json.Linq;

namespace KiCadComponentsManager.Services
{
    public class DigikeyClient
    {
        //private static KiCadSecrets Secrets { get; } = new();
        static string? access_token = null;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Manufacturer part number:");
                add_component(Console.ReadLine());
            }
            else
            {
                add_component(args[0]);
            }
        }


        public static async void Login()
        {
            string url = "https://api.digikey.com/v1/oauth2/token";
            string data = $"client_id={KiCadSecrets.Digikey_client_id}&client_secret={KiCadSecrets.Digikey_client_secret}&grant_type=client_credentials";

            HttpClient client = new();
            StringContent content = new(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            dynamic j = JsonConvert.DeserializeObject(responseBody);
            access_token = j.access_token;
        }

        public static async Task<Dictionary<string, string>> GetComponentInfo(string mpn)
        {
            if (string.IsNullOrEmpty(mpn)) return [];

            string url = $"https://api.digikey.com/products/v4/search/{mpn}/productdetails";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Site", "NL");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Language", "en");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Currency", "EUR");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Client-Id", $"{KiCadSecrets.Digikey_client_id}");

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            dynamic j = JsonConvert.DeserializeObject(responseBody);

            Dictionary<string, string> digikey_parameters = [];
            foreach (dynamic param in j.Product.Parameters)
            {
                digikey_parameters[(string)param.ParameterText] = (string)param.ValueText;
            }

            digikey_parameters["digikey_category"] = j.Product.Category.Name;
            digikey_parameters["manufacturer"] = j.Product.Manufacturer.Name;
            digikey_parameters["description"] = j.Product.Description.ProductDescription;
            digikey_parameters["price1"] = j.Product.UnitPrice;
            digikey_parameters["datasheet"] = j.Product.DatasheetUrl;

            return digikey_parameters;
        }

        static async void add_component(string mpn)
        {
            string url = $"https://api.digikey.com/products/v4/search/{mpn}/productdetails";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue($"Bearer {access_token}");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Site", "NL");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Language", "en");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Locale-Currency", "EUR");
            client.DefaultRequestHeaders.Add("X-DIGIKEY-Client-Id", "aG3n2wZyZ6tasspouf6uEICGcVoYrSZx");

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            dynamic j = JsonConvert.DeserializeObject(responseBody);

            string digikey_category = j.Product.Category.Name;
            string manufacturer = j.Product.Manufacturer.Name;
            string description = j.Product.Description.ProductDescription;
            string price1 = j.Product.UnitPrice;
            string datasheet = j.Product.DatasheetUrl;
            Dictionary<string, string> digikey_parameters = new Dictionary<string, string>();
            foreach (dynamic param in j.Product.Parameters)
            {
                digikey_parameters[(string)param.ParameterText] = (string)param.ValueText;
            }
            string sql = "", table = "", symbol = "", footprint = "";
            switch (digikey_category)
            {
                case "Resistors":
                    symbol += "Resistors:";
                    footprint += "Resistors:";
                    table += "Resistors";
                    string package = digikey_parameters["Supplier Device Package"];
                    string tolerance = digikey_parameters["Tolerance"];
                    string power = digikey_parameters["Power (Watts)"];
                    string value = digikey_parameters["Resistance"].Replace(" ", "").Replace("Ohms", "");
                    switch (package)
                    {
                        case "0402":
                        case "0603":
                        case "0805":
                        case "1206":
                            symbol += "Resistor";
                            footprint += "R_" + package;
                            break;
                    }
                    sql = $"INSERT INTO \"main\".\"{table}\"(\"Manufacturer part number\",\"Manufacturer\",\"Description\",\"Value\",\"Tolerance\",\"Power\",\"Package\",\"Symbol\",\"Footprint\",\"Datasheet\",\"Price1\") " +
                        $"VALUES (\"{mpn}\",\"{manufacturer}\",\"{description}\",\"{value}\",\"{tolerance}\",\"{power}\",\"{package}\",\"{symbol}\",\"{footprint}\",\"{datasheet}\",\"{price1}\");";
                    break;
                case "Capacitors":
                    symbol += "Capacitors:";
                    footprint += "Capacitors:";
                    table = "Capacitors";
                    package = digikey_parameters["Package / Case"];
                    value = digikey_parameters["Capacitance"];
                    tolerance = digikey_parameters["Tolerance"];
                    string dielectric = digikey_parameters["Temperature Coefficient"];
                    string voltage = digikey_parameters["Voltage - Rated"];
                    if (package.Contains('('))
                    {
                        package = package.Substring(0, package.IndexOf("(")).Replace(" ", "");
                    }
                    switch (package)
                    {
                        case "0402":
                        case "0603":
                        case "0805":
                        case "1206":
                            symbol += "Capacitor";
                            footprint += "C_" + package;
                            break;
                    }
                    sql = $"INSERT INTO \"main\".\"{table}\"(\"Manufacturer part number\",\"Manufacturer\",\"Description\",\"Value\",\"Tolerance\",\"Voltage\",\"Dielectric\",\"Package\",\"Symbol\",\"Footprint\",\"Datasheet\",\"Price1\") " +
                        $"VALUES (\"{mpn}\",\"{manufacturer}\",\"{description}\",\"{value}\",\"{tolerance}\",\"{voltage}\",\"{dielectric}\",\"{package}\",\"{symbol}\",\"{footprint}\",\"{datasheet}\",\"{price1}\");";
                    break;
                default:
                    //TODO Let user select category
                    
                    break;
            }
            Console.WriteLine();
            Console.WriteLine(sql);
            Console.WriteLine($"Symbol: {symbol}");
            Console.WriteLine($"Footprint: {footprint}");
            Console.ReadLine();
        }
    }
}
