using IFCConverto.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IFCConverto.Services
{
    public static class HttpService
    {               
        /// <summary>
        /// This method is used to send a POST request to the API with the JSON string
        /// </summary>
        /// <param name="meta">List of meta data for different products</param>
        /// <returns>Custom HttpRequestResponse</returns>
        public static async Task<HttpRequestResponse> Post(ProductData productData, string serverURL)
        {            
            using (HttpClient client = new HttpClient())
            {
                string metadataImportPath = "metadata/import";
                var url = new Uri(string.Format("{0}/{1}", serverURL, metadataImportPath));

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(productData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    string resultString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<HttpRequestResponse>(resultString);                    
                }
                catch (Exception ex)
                {
                    throw ex;                    
                }
            }                        
        }

        public static async Task<HttpRequestResponse> PostModelLinks(ProductData productData, string serverURL)
        {            
            using (HttpClient client = new HttpClient())
            {
                string imageImportPath = "threedmodel/import";                
                var url = new Uri(string.Format("{0}/{1}", serverURL, imageImportPath));

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(productData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    string resultString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<HttpRequestResponse>(resultString);                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
