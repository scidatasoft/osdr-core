using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Domain;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.RecordsFile.Domain;
using Sds.Osdr.Spectra.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests
{
    public class ValidJdxProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public ValidJdxProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "2-Methyl-1-Propanol.jdx", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileRecordsFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class ValidJdxProcessing : OsdrTest, IClassFixture<ValidJdxProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public ValidJdxProcessing(OsdrTestHarness fixture, ITestOutputHelper output, ValidJdxProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
        public async Task SpectrumProcessing_ValidJdx_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_GeneratesExpectedFileAggregate()
		{
			var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
			blobInfo.Should().NotBeNull();

			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

			file.Should().NotBeNull();
			file.Should().Should().BeEquivalentTo(new
			{
				Id = FileId,
				Type = FileType.Records,
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
				Status = FileStatus.Processed,
				TotalRecords = 1,
				Fields = new string[] { "Field1", "Field2" }
			}, options => options
				.ExcludingMissingMembers()
			);
			file.Images.Should().BeEmpty();
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_ExpectedFileEntity()
		{
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);
            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

            fileView.Should().EntityShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_ExpectedFileNode()
		{
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);
            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;

			fileNode.Should().NotBeNull();
			fileNode.Should().NodeShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public void SpectrumProcessing_ValidJdx_RecordsOnlyOne()
		{
            var records = Harness.GetProcessedRecords(FileId);
            records.Should().HaveCount(1);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_ExpectedRecordEntity()
		{
			var recordId = Harness.GetProcessedRecords(FileId).First();
			var record = await Session.Get<Spectrum>(recordId);
			var recordView = Records.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;

			recordView.Should().NotBeNull();
			recordView.Should().EntityShouldBeEquivalentTo(record);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_ExpectedRecordNode()
		{
			var recordId = Harness.GetProcessedRecords(FileId).First();
			var record = await Session.Get<Spectrum>(recordId);
			var recordView = Records.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;

			var recordNode = Nodes.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;
			recordNode.Should().NotBeNull();
			recordNode.Should().NodeShouldBeEquivalentTo(record);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Spectrum)]
		public async Task SpectrumProcessing_ValidJdx_GeneratesExpectedRecordAggregate()
		{
			var recordId = Harness.GetProcessedRecords(FileId).First();
			var record = await Session.Get<Spectrum>(recordId);
			record.Should().NotBeNull();
			record.Should().Should().BeEquivalentTo(new
			{
				Id = recordId,
				RecordType = RecordType.Spectrum,
				Bucket = JohnId.ToString(),
				OwnedBy = JohnId,
				CreatedBy = JohnId,
				CreatedDateTime = DateTimeOffset.UtcNow,
				UpdatedBy = JohnId,
				UpdatedDateTime = DateTimeOffset.UtcNow,
				ParentId = FileId,
				Status = RecordStatus.Processed,
				Fields = new Field[] {
					new Field("Field1", "Value1"),
					new Field("Field2", "Value2")
				},
				Issues = new Issue[] { }
			}, options => options
				.ExcludingMissingMembers()
			);
			record.Images.Should().BeEmpty();
			record.Properties.Should().NotBeNull();
			record.Properties.Should().HaveCount(0);
		}
    }
}
