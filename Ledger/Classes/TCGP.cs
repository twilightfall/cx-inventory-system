using Ledger.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Classes
{
    public class TCGAuth
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
        public string UserName { get; set; }

        [JsonProperty(PropertyName =".issued")]
        public string Issued { get; set; }

        [JsonProperty(PropertyName = ".expires")]
        public string Expires { get; set; }

        public TCGAuth()
        {

        }
        
        public static async Task<TCGAuth> GetBearerToken()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.tcgplayer.com/token"))
                {
                    string content = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}", Resources.ClientId, Resources.ClientSecret);

                    request.Content = new StringContent(content);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                    var tokres = await response.Content.ReadAsStringAsync();
                    var t = JsonConvert.DeserializeObject<TCGAuth>(tokres);
                    return t;
                }
            }
        }
    }

    public class Result
    {
        public int ProductId { get; set; }
        public double? LowPrice { get; set; }
        public double? MidPrice { get; set; }
        public double? HighPrice { get; set; }
        public double? MarketPrice { get; set; }
        public double? DirectLowPrice { get; set; }
        public string SubTypeName { get; set; }
    }

    public class TCGPrice
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public List<Result> Results { get; set; }
    }

    public class Price
    {
        public int ID { get; set; }
        public double MidPrice { get; set; }
    }
}
