using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests.Tests.Blobs
{
    public class CreatePublicLinkForEntityImageFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; }

        public CreatePublicLinkForEntityImageFixture(OsdrWebTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Aspirin.mol", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileRecordsFileProcessed(BlobId);

            var file = harness.Session.Get<RecordsFile.Domain.RecordsFile>(FileId).Result;
            var response = harness.JohnApi.SetPublicFileEntity__New(FileId, file.Version, true).Result;
            harness.WaitWhileFileShared(FileId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class CreatePublicLinkForEntityImage : OsdrWebTest, IClassFixture<CreatePublicLinkForEntityImageFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public CreatePublicLinkForEntityImage(OsdrWebTestHarness fixture, ITestOutputHelper output, CreatePublicLinkForEntityImageFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Sharing)]
        public async Task FileSharing_WithAuthorizeUser_ReturnsExpectedImage()
        {
            var fileResponse = await JohnApi.GetFileEntityById(FileId);
            var file = JObject.Parse(await fileResponse.Content.ReadAsStringAsync());
            var imageId = file["images"].First()["id"].ToObject<Guid>();

            var blobResponse = await JohnApi.GetImagesFileEntityById(FileId, imageId);
            blobResponse.EnsureSuccessStatusCode();
            blobResponse.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
            blobResponse.Content.Headers.ContentType.MediaType.ShouldBeEquivalentTo("application/octet-stream");
            blobResponse.Content.Headers.ContentLength.Should().BeGreaterThan(10000);
            blobResponse.Content.Headers.ContentDisposition.FileName.ShouldBeEquivalentTo("Aspirin.mol.svg");
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Sharing)]
        public async Task FileSharing_WithUnauthorizeUser_ReturnsExpectedImage()
        {
            var fileResponse = await JohnApi.GetFileEntityById(FileId);
            var file = JObject.Parse(await fileResponse.Content.ReadAsStringAsync());
            var imageId = file["images"].First()["id"].ToObject<Guid>();

            var blobResponse = await UnauthorizedApi.GetImagesFileEntityById(FileId, imageId);
            blobResponse.EnsureSuccessStatusCode();
            blobResponse.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
            blobResponse.Content.Headers.ContentType.MediaType.ShouldBeEquivalentTo("application/octet-stream");
            blobResponse.Content.Headers.ContentLength.Should().BeGreaterThan(10000);
            blobResponse.Content.Headers.ContentDisposition.FileName.ShouldBeEquivalentTo("Aspirin.mol.svg");
        }
    }
}