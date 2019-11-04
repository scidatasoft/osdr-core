using Flurl;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests
{
    public class Token
    {
        public string access_token { get; set; }
    }

    public class UserInfo
    {
        public Guid sub { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string preferred_username { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }

    public class KeyCloalClient : HttpClient
    {
        public string Authority { get; }
        public string ClientId { get; } = "osdr_webapi";
        public string ClientSecret { get; } = "52f5b3fc-2167-40b1-9508-6bb3091782bd";

        public KeyCloalClient() : base()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();
            Authority = Environment.ExpandEnvironmentVariables(configuration["KeyCloak:Authority"]);
        }

        public async Task<Token> GetToken(string username, string password)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("username", username));
            nvc.Add(new KeyValuePair<string, string>("password", password));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "password"));

            Log.Debug($"GetToken({username}, {password})");
            Log.Debug($"URL: {Url.Combine(Authority, "protocol/openid-connect/token")}");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(Url.Combine(Authority, "protocol/openid-connect/token"))) { Content = new FormUrlEncodedContent(nvc) };

            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}")));

            var response = await SendAsync(request);

            Log.Debug($"Response: {response})");

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Token>(json);
        }

        public async Task<Token> GetClientToken(string clientId, string secret)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(Url.Combine(Authority, "protocol/openid-connect/token"))) { Content = new FormUrlEncodedContent(nvc) };

            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}")));

            var response = await SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Token>(json);
        }

        public async Task<UserInfo> GetUserInfo(string username, string password)
        {
            var token = await GetToken(username, password);

            return await GetUserInfo(token.access_token);
        }

        public async Task<UserInfo> GetUserInfo(string token)
        {
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await GetAsync(new Uri(Url.Combine(Authority, "protocol/openid-connect/userinfo")));

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserInfo>(json);
        }
    }
}
