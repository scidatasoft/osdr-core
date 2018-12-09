using Sds.Osdr.IntegrationTests;
using Sds.Osdr.WebApi.IntegrationTests.EndPoints;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class OsdrWebTestHarness : OsdrTestHarness
    {
        private HttpClient JohnClient { get; }
        private HttpClient JaneClient { get; }
        private HttpClient UnauthorizedClient { get; }

        public OsdrWebClient JohnApi { get; }
        public OsdrWebClient JaneApi { get; }
        public OsdrWebClient UnauthorizedApi { get; }

        public OsdrWebTestHarness() : base()
        {
            var token = keycloak.GetToken("john", "qqq123").Result;

            JohnClient = new HttpClient();
            JohnClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

            token = keycloak.GetToken("jane", "qqq123").Result;

            JaneClient = new HttpClient();
            JaneClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

            UnauthorizedClient = new HttpClient();

            JohnApi = new OsdrWebClient(JohnClient);
            JaneApi = new OsdrWebClient(JaneClient);
            UnauthorizedApi = new OsdrWebClient(UnauthorizedClient);
        }

        public override void Dispose()
        {
            JohnClient.Dispose();
            JaneClient.Dispose();
            UnauthorizedClient.Dispose();

            base.Dispose();
        }
    }
}