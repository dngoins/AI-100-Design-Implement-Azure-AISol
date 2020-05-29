using System;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace DetermineSentiment
{
    class Program
    {
        static  void Main(string[] args)
        {
            DetermineSentiment();

            // last line needed otherwise
            // app terminates without result due to 
            // await async behavior
            Console.ReadLine();
        }

        static async void DetermineSentiment()
        {
            Console.WriteLine("Hello Welcome to the Sentiment Detector...!");
            Console.WriteLine("Please enter in a phrase so I can detect positive or negative vibes");
            var phrase = Console.ReadLine();

            var client = new HttpClient();
            var queryStr = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "ecf27f76e2c140f5a6bceffce3967d91");

            var uri = "https://eastus.cognitiveservices.azure.com/text/analytics/v2.0/sentiment?" + queryStr;
           
            HttpResponseMessage response;
            string bodyTemplate = "{ \"documents\": [{\"language\": \"en\",\"id\": \"1\",\"text\": \"@phrase\" }]}";

            string body = bodyTemplate.Replace("@phrase", phrase);

            byte[] byteData = Encoding.UTF8.GetBytes(body);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                try
                {
                    response = await client.PostAsync(uri, content);
                    Console.WriteLine("Status Code: {0}\nContent:\n{1}", response.StatusCode, await response.Content.ReadAsStringAsync());

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                }
           
        }
    }
}
