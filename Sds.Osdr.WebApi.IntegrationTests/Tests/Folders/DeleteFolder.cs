using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
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
    public class DeleteFolder : OsdrWebTest
    {
        private Guid _folderId;

        public DeleteFolder(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            var response = JohnApi.CreateFolderEntity(JohnId, "new folder").Result;
            var folderLocation = response.Headers.Location.ToString();
            _folderId = Guid.Parse(folderLocation.Substring(folderLocation.LastIndexOf("/") + 1));

            Harness.WaitWhileFolderCreated(_folderId);
        }

        [Fact(Skip = "Need to think"), WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task FolderOperation_DeleteFolder_FolderIsDeleted()
        {
            var response = await JohnApi.GetFolder(_folderId);
	        response.EnsureSuccessStatusCode();
	        var jsonFolder = JToken.Parse(await response.Content.ReadAsStringAsync());
            
            await JohnApi.DeleteFolder(_folderId, jsonFolder["version"].ToObject<int>());
            Harness.WaitWhileFolderDeleted(_folderId);

            var notFoundResponse = await JohnApi.GetFolder(_folderId);
	        notFoundResponse.IsSuccessStatusCode.ShouldBeEquivalentTo(false);
            //Should not return Forbid status code
            response.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.Forbidden);
//            responseBeNotFoundFolder.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.NotFound);
        }
    }
}