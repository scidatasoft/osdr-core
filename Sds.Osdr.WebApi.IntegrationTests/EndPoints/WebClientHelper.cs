using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sds.Osdr.WebApi.IntegrationTests.EndPoints
{
    public class OsdrWebClient
    {
        private HttpClient client;
        private Uri BaseUri { get; }

        public OsdrWebClient(HttpClient client)
        {
            this.client = client;
            BaseUri = new Uri(Environment.ExpandEnvironmentVariables("%OSDR_WEB_API%"));
        }

        public async Task<HttpResponseMessage> GetData(string url, string contentType = "application/json")
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), new Uri(BaseUri, url));

            request.Content = new StringContent("");

            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var response = await client.SendAsync(request);

            return response;
        }

        public async Task<HttpResponseMessage> DeleteData(string url)
        {
            var request = new HttpRequestMessage(new HttpMethod("DELETE"), new Uri(BaseUri, url));

            request.Content = new StringContent("");

            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.SendAsync(request);

            return response;
        }

        public async Task<HttpResponseMessage> PatchData(string url, string stringContent)
        {
            var content = new StringContent(stringContent);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(BaseUri, url))
            {
                Content = content
            };

            HttpResponseMessage response = new HttpResponseMessage();

            response = await client.SendAsync(request);

            return response;
        }

        public async Task<HttpResponseMessage> PutData(string url, string stringContent)
        {
            var content = new StringContent(stringContent);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(new HttpMethod("PUT"), new Uri(BaseUri, url))
            {
                Content = content
            };

            HttpResponseMessage response = new HttpResponseMessage();

            response = await client.SendAsync(request);

            return response;
        }


        public async Task<HttpResponseMessage> PutData(string url, object data)
        {
            return await PutData(url, JsonConvert.SerializeObject(data));
        }

        public async Task<HttpResponseMessage> PostData(string url, string data)
        {
            var httpContent = new StringContent(data);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(BaseUri, url), httpContent);

            return  response;
        }

        public async Task<HttpResponseMessage> PostData(string url, object data)
        {
            return await PostData(url, JsonConvert.SerializeObject(data));
        }
    }
}