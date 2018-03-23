using System;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;

namespace APIClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("tenant - " + ConfigurationSettings.AppSettings["tenant"]);
            Console.WriteLine("clientid - " + ConfigurationSettings.AppSettings["clientid"]);
            Console.WriteLine("resourceuri - " + ConfigurationSettings.AppSettings["resourceuri"]);
            Console.WriteLine("authorityuri - " + ConfigurationSettings.AppSettings["authorityuri"]);
            Console.WriteLine("redirecturi - " + ConfigurationSettings.AppSettings["redirecturi"]);

            var t = GetTokenAsync();
            t.Wait();
            string token = t.Result;

            //Your request to test 
            string url = "https://host/api/operation?parameter=value";

            try
            {
                var r = CallApiAsync(url, token);
                r.Wait();
                Console.WriteLine("\n\nHTTP response status: " + r.Status);
                Console.WriteLine(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(r.Result), Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Make http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task<string> CallApiAsync(string url, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.GetStringAsync(url);
                return response;
            }
        }

        /// <summary>
        /// Request token to authentication platform
        /// </summary>
        /// <returns></returns>
        static async Task<string> GetTokenAsync()
        {
            string tenant = ConfigurationSettings.AppSettings["tenant"];
            string clientId = ConfigurationSettings.AppSettings["clientid"];
            string resourceUri = ConfigurationSettings.AppSettings["resourceuri"];
            Uri redirectUri = new Uri(ConfigurationSettings.AppSettings["redirecturi"]);
            string authorityUri = String.Format(CultureInfo.InvariantCulture, ConfigurationSettings.AppSettings["authorityuri"], tenant);

            AuthenticationContext context = new AuthenticationContext(authorityUri);
            PlatformParameters platformParams = new PlatformParameters(PromptBehavior.Auto, null);
            AuthenticationResult result = await context.AcquireTokenAsync(resourceUri, clientId, redirectUri, platformParams);
            Console.WriteLine("\n\nToken acquired");
            Console.WriteLine("AccessTokenType: " + result.AccessTokenType);
            Console.WriteLine("Authority: " + result.Authority);
            Console.WriteLine("ExpiresOn: " + result.ExpiresOn);
            Console.WriteLine("AccessToken: " + result.AccessToken);

            return result.AccessToken;
        }
    }
}
