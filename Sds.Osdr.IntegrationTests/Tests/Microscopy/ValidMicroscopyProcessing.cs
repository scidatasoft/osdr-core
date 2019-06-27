using FluentAssertions;
using Leanda.Microscopy.Domain;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests
{
    public class ValidMicroscopyProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public ValidMicroscopyProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Nikon_BF007.nd2", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class ValidMicroscopyProcessing : OsdrTest, IClassFixture<ValidMicroscopyProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public ValidMicroscopyProcessing(OsdrTestHarness fixture, ITestOutputHelper output, ValidMicroscopyProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Microscopy)]
        public async Task MicroscopyProcessing_ValidNd2_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Microscopy)]
        public async Task MicroscopyProcessing_ValidNd2_GeneratesAppropriateModels()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var file = await Session.Get<MicroscopyFile>(FileId);
            file.Should().NotBeNull();
            file.ShouldBeEquivalentTo(new
            {
                Id = FileId,
                Type = FileType.Microscopy,
                Bucket = JohnId.ToString(),
                BlobId = BlobId,
                PdfBucket = file.Bucket,
                OwnedBy = JohnId,
                CreatedBy = JohnId,
                CreatedDateTime = DateTimeOffset.UtcNow,
                UpdatedBy = JohnId,
                UpdatedDateTime = DateTimeOffset.UtcNow,
                ParentId = JohnId,
                FileName = blobInfo.FileName,
                Length = blobInfo.Length,
                Md5 = blobInfo.MD5,
                IsDeleted = false,
                Status = FileStatus.Processed
            }, options => options
                .ExcludingMissingMembers()
            );
            file.Images.Should().NotBeNullOrEmpty();
            file.Images.Should().HaveCount(3);
        }
        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Microscopy)]
        public async Task MicroscopyProcessing_ValidNd2_ExpectedFileEntity()
        {
            var file = await Session.Get<MicroscopyFile>(FileId);
            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileView.Should().EntityShouldBeEquivalentTo(file);
        }
        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Microscopy)]
        public async Task MicroscopyProcessing_ValidNd2_ExpectedFileNode()
        {
            var file = await Session.Get<MicroscopyFile>(FileId);
            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileNode.Should().NodeShouldBeEquivalentTo(file);
        }
    }
}
