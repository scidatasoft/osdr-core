using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.Pdf.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests
{
    public class PdfProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public PdfProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Abdelaziz A Full_manuscript.pdf", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class PdfProcessing : OsdrTest, IClassFixture<PdfProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public PdfProcessing(OsdrTestHarness fixture, ITestOutputHelper output, PdfProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Pdf)]
        public async Task PdfProcessing_ValidPdf_GeneratesAppropriateModels()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var file = await Session.Get<PdfFile>(FileId);
            file.Should().NotBeNull();
            file.Should().Should().BeEquivalentTo(new
            {
                Id = FileId,
                Type = FileType.Pdf,
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
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Pdf)]
        public async Task PdfProcessing_ValidPdf_ExpectedFileEntity()
        {
            var file = await Session.Get<PdfFile>(FileId);
            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileView.Should().EntityShouldBeEquivalentTo(file);
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Pdf)]
        public async Task PdfProcessing_ValidPdf_ExpectedFileNode()
        {
            var file = await Session.Get<PdfFile>(FileId);
            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileNode.Should().NodeShouldBeEquivalentTo(file);
        }
    }
}
