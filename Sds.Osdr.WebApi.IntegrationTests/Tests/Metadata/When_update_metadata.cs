using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    [Collection("OSDR Test Harness")]
    public class UpdateMetadata : OsdrWebTest
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public UpdateMetadata(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
		{
            BlobId = fixture.JohnBlobStorageClient.AddResource(fixture.JohnId.ToString(), "2018-02-14.gif", new Dictionary<string, object>() { { "parentId", fixture.JohnId } }).Result;

            FileId = fixture.WaitWhileFileProcessed(BlobId);
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
		public async Task UpdateMetadata_UpdateGenericMetadata_ExpectedRenamedFolder()
		{
            var processedFile = await Harness.Session.Get<File>(FileId);

            var url = $"/api/entities/files/{FileId}?version={processedFile.Version}";
            var data = $"[{{'op':'replace','path':'/Permissions/Metadata','value':[{{test1: 'value1'}}]}}]";

            JohnApi.PatchData(url, data).Wait();

            //await JohnApi.RenameFolder(_folderId, "renamed folder");

            //Harness.WaitWhileFolderRenamed(_folderId);

            //var response = await JohnApi.GetFolder(_folderId);
            //         response.EnsureSuccessStatusCode();
            //         var jsonFolder = JToken.Parse(await response.Content.ReadAsStringAsync());

            //jsonFolder.Should().ContainsJson($@"
            //{{
            //	'id': '{_folderId}',
            //	'createdBy': '{JohnId}',
            //	'createdDateTime': *EXIST*,
            //	'updatedBy': '{JohnId}',
            //	'updatedDateTime': *EXIST*,
            //	'ownedBy': '{JohnId}',
            //	'name': 'renamed folder',
            //	'status': 'Created',
            //	'version': 2
            //}}");
        }
    }
}
