using Ledger.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Ledger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SetList root;
        public static string connstring = $@"Data Source={Environment.MachineName}\SQLEXPRESS;Initial Catalog=cxdb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //public static string connstring = @"Data Source=COW-001\SQLEXPRESS;Initial Catalog=cxdb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //public static string ClientId = "C405179A-08CE-42E8-B80F-8AA8628C8BCE";
        //public static string ClientSec = "7F6F4CDA-5016-47E7-86EE-BFA4184C7436";
        public static string Bearer;

        public static async Task<SetList> GetSetObjects()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.scryfall.com/sets"))
                {
                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    return root = JsonConvert.DeserializeObject<SetList>(tokres);
                }
            }
        }

        public static async Task<TCGPrice> GetPrice(int ProdId)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.36.0/pricing/product/" + ProdId))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer " + Bearer);
                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject<TCGPrice>(tokres);

                    return t;
                }
            }
        }
    }
}
