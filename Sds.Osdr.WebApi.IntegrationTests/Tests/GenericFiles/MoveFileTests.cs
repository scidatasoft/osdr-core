using FluentAssertions;
using Newtonsoft.Json;
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

namespace Sds.Osdr.WebApi.IntegrationTests.GenericFiles
{
    public class MoveFileFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }
        public Guid FolderId { get; set; }

        public MoveFileFixture(OsdrWebTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Chemical-diagram.png", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);

            var folderResponse = harness.JohnApi.CreateFolderEntity(harness.JohnId, "test1").Result;
            var folderLocation = folderResponse.Headers.Location.ToString();
            FolderId = Guid.Parse(folderLocation.Substring(folderLocation.LastIndexOf("/") + 1));
            var file = harness.Session.Get<File>(FileId).Result;
            var response = harness.JohnApi.SetParentFolder__New(FileId, file.Version, FolderId).Result;
            harness.WaitWhileFileMoved(FileId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class MoveFileTests : OsdrWebTest, IClassFixture<MoveFileFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }
        public Guid FolderId { get; set; }

        public MoveFileTests(OsdrWebTestHarness fixture, ITestOutputHelper output, MoveFileFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
            FolderId = initFixture.FolderId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Generic)]
        public async Task PngUpload_ValidPng_GenerateExpectedFileEntity()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var fileEntityResponse = await JohnApi.GetFileEntityById(FileId);
            var fileEntity = JsonConvert.DeserializeObject<JObject>(await fileEntityResponse.Content.ReadAsStringAsync());
            fileEntity.Should().NotBeNull();

            fileEntity.Should().ContainsJson($@"
			{{
				'id': '{FileId}',
				'blob': {{
					'id': '{blobInfo.Id}',
					'bucket': '{JohnId}',
					'length': {blobInfo.Length},
					'md5': '{blobInfo.MD5}'
				}},
				'subType': '{FileType.Image}',
				'ownedBy': '{JohnId}',
				'createdBy': '{JohnId}',
				'createdDateTime': '{DateTime.UtcNow}',
				'updatedBy': '{JohnId}',
				'updatedDateTime': '{DateTime.UtcNow}',
				'parentId': '{FolderId}',
				'name': '{blobInfo.FileName}',
				'status': '{FileStatus.Processed}',
				'version': *EXIST*
			}}");
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Generic)]
        public async Task PngUpload_ValidPng_GenerateExpectedFileNode()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            fileNode.Should().ContainsJson($@"
			{{
				'id': '{FileId}',
				'type': 'File',
				'subType': 'Image',
				'blob': {{
					'id': '{blobInfo.Id}',
					'bucket': '{JohnId}',
					'length': {blobInfo.Length},
					'md5': '{blobInfo.MD5}'
				}},
				'status': '{FileStatus.Processed}',
				'ownedBy':'{JohnId}',
				'createdBy': '{JohnId}',
				'createdDateTime': '{DateTime.UtcNow}',
				'updatedBy': '{JohnId}',
				'updatedDateTime': '{DateTime.UtcNow}',
				'name': '{blobInfo.FileName}',
				'parentId': '{FolderId}',
				'version': *EXIST*
			}}");
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Generic)]
        public async Task PngUpload_ValidPng_GenerateExpectedRecordNodesOnlyEmpty()
        {
            var recordResponse = await JohnApi.GetNodesById(FileId);
            var recordNodes = JsonConvert.DeserializeObject<JArray>(await recordResponse.Content.ReadAsStringAsync());

            recordNodes.Should().HaveCount(0);
        }
    }
}