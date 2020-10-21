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
        /// <returns>True or false based on the operation status</returns>
        public static async Task<bool> Post(ProductData productData, string serverURL)
        {            
            string requestStatus = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                // update this URL
                //var url = "https://httpbin.org/post";
                var url = serverURL;

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(productData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    if(response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }                        
        }

        public static async Task<bool> PostModelLinks(ProductData productData, string serverURL)
        {
            string requestStatus = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                // update this URL
                //var url = "https://httpbin.org/post";
                var url = serverURL;

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(productData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
