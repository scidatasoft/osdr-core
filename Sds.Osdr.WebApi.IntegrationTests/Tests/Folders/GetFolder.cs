using FluentAssertions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    [Collection("OSDR Test Harness")]
    public class GetFolder : OsdrWebTest
    {
        public GetFolder(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Fact(Skip = "Need to think"), WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task FolderOperation_GetAccessToNotExistingFolder_ReturnsNotFound()
        {
            var response = await JohnApi.GetFolder(Guid.NewGuid());
	        response.EnsureSuccessStatusCode();
            //Should not return Forbid status code
            response.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.NotFound);
        }
    }
}