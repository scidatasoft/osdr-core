using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.Tabular.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests
{
    public class CsvProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public CsvProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class CsvProcessing : OsdrTest, IClassFixture<CsvProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public CsvProcessing(OsdrTestHarness fixture, ITestOutputHelper output, CsvProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Tabular)]
        public async Task TabularProcessing_ValidCsv_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Tabular)]
        public async Task TabularProcessing_ValidCsv_GeneratesExpectedFileAggregate()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var file = await Session.Get<TabularFile>(FileId);
			file.Should().NotBeNull();
			file.Should().Should().BeEquivalentTo(new
            {
                Id = FileId,
                Type = FileType.Tabular,
                Bucket = JohnId.ToString(),
                BlobId = BlobId,
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

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Tabular)]
		public async Task TabularProcessing_ValidCsv_ExpectedFileEntity()
		{
            var file = await Session.Get<TabularFile>(FileId);
            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileView.Should().EntityShouldBeEquivalentTo(file);
		}
		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Tabular)]
		public async Task TabularProcessing_ValidCsv_ExpectedFileNode()
		{
            var file = await Session.Get<TabularFile>(FileId);
            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileNode.Should().NodeShouldBeEquivalentTo(file);
		}
    }
}
