using FluentAssertions;
using Sds.Osdr.IntegrationTests.Traits;
using System.Threading.Tasks;
using Xunit;

namespace Sds.Osdr.IntegrationTests.Tests
{
    public class KeyCloakTests
    {
        [Fact, ProcessingTrait(TraitGroup.All)]
        public async Task KeyCloak_AuthenticateUser_ReturnsValidToken()
        {
            using (var keycloak = new KeyCloalClient())
            {
                var token = await keycloak.GetToken("john", "qqq123");

                token.access_token.Should().NotBeNullOrEmpty();
            }
        }

        [Fact, ProcessingTrait(TraitGroup.All)]
        public async Task KeyCloak_AuthenticatedUser_CanGetAccessToUserInfo()
        {
            using (var keycloak = new KeyCloalClient())
            {
                var token = await keycloak.GetToken("john", "qqq123");

                token.access_token.Should().NotBeNullOrEmpty();

                var userInfo = await keycloak.GetUserInfo(token.access_token);

                userInfo.given_name.Should().Be("John");
                userInfo.family_name.Should().Be("Doe");
            }
        }

        [Fact, ProcessingTrait(TraitGroup.All)]
        public async Task KeyCloak_AuthenticateClient_ReturnsValidToken()
        {
            using (var keycloak = new KeyCloalClient())
            {
                var token = await keycloak.GetClientToken("osdr_ml_modeler", "osdr_ml_modeler_secret");

                token.access_token.Should().NotBeNullOrEmpty();
            }
        }

        //[Fact(Skip = "Need to make sure that KeyCloak doesn't return USer info for Client authentication"), ProcessingTrait(TraitGroup.All)]
        //public async Task KeyCloak_AuthenticatedClient_ShouldNotReturnValidUserInfo()
        //{
        //    using (var keycloak = new KeyCloalClient())
        //    {
        //        var token = await keycloak.GetClientToken("osdr_ml_modeler", "osdr_ml_modeler_secret");

        //        token.access_token.Should().NotBeNullOrEmpty();

        //        var userInfo = await keycloak.GetUserInfo(token.access_token);

        //    }
        //}
    }
}
