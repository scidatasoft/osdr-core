using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Chemicals.Domain.Aggregates;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.Generic.Domain.ValueObjects;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.RecordsFile.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests
{
    public class ValidSdfProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public ValidSdfProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "drugbank_10_records.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileRecordsFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class ValidSdfProcessing : OsdrTest, IClassFixture<ValidSdfProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public ValidSdfProcessing(OsdrTestHarness fixture, ITestOutputHelper output, ValidSdfProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
		public async Task ChemicalProcessing_ValidSdf_GenerateExpectedFileAggregate()
		{
			var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
			blobInfo.Should().NotBeNull();

			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

			file.Should().NotBeNull();
			file.Should().BeEquivalentTo(new
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
				TotalRecords = 2,
				Fields = new List<string>() { "StdInChI", "StdInChIKey", "SMILES" }
			}, options => options
				.ExcludingMissingMembers()
			);
			file.Images.Should().NotBeNullOrEmpty();
			file.Images.Count.Should().Be(1);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
		public async Task ChemicalProcessing_ValidSdf_GenerateExpectedFileEntity()
		{
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;
            fileView.Should().EntityShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
		public async Task ChemicalProcessing_ValidSdf_GenerateExpectedFileNode()
		{
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;
			fileNode.Should().NotBeNull();
			fileNode.Should().NodeShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
		public void ChemicalProcessing_ValidSdf_GenerateOnlyTwoRecords()
		{
            var recordIds = Harness.GetProcessedRecords(FileId);
            recordIds.Should().HaveCount(2);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
		public async Task ChemicalProcessing_ValidSdf_GenerateSubstanceAggregate()
		{
            var recordIds = Harness.GetProcessedRecords(FileId);

            foreach (var recordId in recordIds)
            {
                var recordView = Records.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;
                recordView.Should().NotBeNull();

                var recordBlob = recordView["Blob"];
				recordBlob.Should().NotBeNull();
				recordBlob.Should().BeAssignableTo<IDictionary<string, object>>();

                var recordBlobId = (recordBlob as IDictionary<string, object>)["_id"];
				recordBlobId.Should().NotBeNull();
				recordBlobId.Should().BeOfType<Guid>();

                var index = Convert.ToInt32(recordView["Index"]);
                index.Should().BeGreaterOrEqualTo(0);

                var record = await Session.Get<Substance>(recordId);
                record.Should().NotBeNull();
                record.Should().BeEquivalentTo(new
                {
                    Id = recordId,
                    RecordType = RecordType.Structure,
                    Bucket = JohnId.ToString(),
                    BlobId = recordBlobId,
                    OwnedBy = JohnId,
                    CreatedBy = JohnId,
                    CreatedDateTime = DateTimeOffset.UtcNow,
                    UpdatedBy = JohnId,
                    UpdatedDateTime = DateTimeOffset.UtcNow,
                    ParentId = FileId,
                    Status = RecordStatus.Processed,
                    Index = index,
                    //Issues = new List<Generic.Domain.ValueObjects.Issue>() { new Generic.Domain.ValueObjects.Issue { Code = "Code", AuxInfo = "AuxInfo", Message = "Message", Severity = Severity.Information, Title = "Title" } }
                    Issues = new List<Generic.Domain.ValueObjects.Issue>() { }
                }, options => options
                    .ExcludingMissingMembers()
                );
				record.Images.Should().NotBeNullOrEmpty();
				record.Images.Should().ContainSingle();
				record.Fields.Should().NotBeNullOrEmpty();
				record.Fields.Should().HaveCount(3);
				record.Properties.Should().NotBeNullOrEmpty();
				record.Properties.Should().HaveCount(9);
            }
		}
        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_GenerateExpectedRecordEntity()
        {
            var recordId = Harness.GetProcessedRecords(FileId).First();
            var record = await Session.Get<Substance>(recordId);

            var recordView = Records.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;
            recordView.Should().EntityShouldBeEquivalentTo(record);
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_GenerateExpectedRecordNode()
        {
            var recordId = Harness.GetProcessedRecords(FileId).First();
            var record = await Session.Get<Substance>(recordId);

            var recordNode = Nodes.Find(new BsonDocument("_id", recordId)).FirstOrDefault() as IDictionary<string, object>;
			recordNode.Should().NotBeNull();
			recordNode.Should().NodeShouldBeEquivalentTo(record);
        }
    }
}
