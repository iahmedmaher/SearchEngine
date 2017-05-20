using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    public class Clairf
    {
        private readonly static string ACCESS_TOKEN = "2ajFHXvliJYCtEdogJX514ZoXDZIq1";
        private static string CLARIFAI_API_URL = "https://api.clarifai.com/v2/models/aaa03c23b3724a16a56b629203edc62c/outputs";
        private HttpClient client;

        public Clairf()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ACCESS_TOKEN);
        }

      
        public String[] getImageTags(string imgUrl)
        {
            String[] result;    
            HttpContent json = new StringContent(
            "{" +
                "\"inputs\": [" +
                    "{" +
                        "\"data\": {" +
                            "\"image\": {" +
                                "\"url\": \"" + imgUrl + "\"" +
                            "}" +
                       "}" +
                    "}" +
                "]" +
            "}", Encoding.UTF8, "application/json");

            var response = client.PostAsync(CLARIFAI_API_URL, json).Result;
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            try
            {
                string body = response.Content.ReadAsStringAsync().Result;
                var yo = JObject.Parse("{\"status\": 10 }");

                var jo = JObject.Parse(body);

                var concepts = ((JArray)jo["outputs"])[0]["data"]["concepts"] as JArray;

                result = new String[concepts.Count];
                for (int i = 0; i < concepts.Count; i++)
                {
                    result[i] = concepts[i]["name"].ToString();
                }
            }
            catch(Exception e)
            {
                return null;
            }

            return result;
            
        }
    }
}
