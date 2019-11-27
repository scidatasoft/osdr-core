using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Sds.Osdr.WebApi.IntegrationTests.EndPoints;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sds.Osdr.EndToEndTests
{
    public class OsdrTestHarness : Sds.Osdr.IntegrationTests.OsdrTestHarness
    {
        private HttpClient JohnClient { get; }
        private HttpClient JaneClient { get; }
        private HttpClient UnauthorizedClient { get; }

        public OsdrWebClient JohnApi { get; }
        public OsdrWebClient JaneApi { get; }
        public OsdrWebClient UnauthorizedApi { get; }

        public OsdrTestHarness() : base()
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

        protected override void OnInit(IServiceCollection services)
        {
        }

        protected override void OnBusCreation(IRabbitMqBusFactoryConfigurator config, IRabbitMqHost host, IServiceProvider container)
        {
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