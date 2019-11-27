﻿using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Crystals.Domain;
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
    public class InvalidCifProcessingImageGenerationFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public InvalidCifProcessingImageGenerationFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "InvalidImageGeneration.cif", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileRecordsFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class InvalidCifProcessingImageGeneration : OsdrTest, IClassFixture<InvalidCifProcessingImageGenerationFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public InvalidCifProcessingImageGeneration(OsdrTestHarness fixture, ITestOutputHelper output, InvalidCifProcessingImageGenerationFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
        public async Task CrystalProcessing_InvalidCif_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
		public async Task CrystalProcessing_InvalidCif_GenerateExpectedFileAggregate()
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
				TotalRecords = 1
			}, options => options
				.ExcludingMissingMembers()
			);
			file.Images.Should().BeEmpty();
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
		public async Task CrystalProcessing_InvalidCif_GenerateExpectedFileEntity()
		{
            var fileView = Files.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

            fileView.Should().NotBeNull();
            fileView.Should().EntityShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
		public async Task CrystalProcessing_InvalidCif_GenerateExpectedFileNode()
		{
			var file = await Session.Get<RecordsFile.Domain.RecordsFile>(FileId);

            var fileNode = Nodes.Find(new BsonDocument("_id", FileId)).FirstOrDefault() as IDictionary<string, object>;
			fileNode.Should().NotBeNull();
			fileNode.Should().NodeShouldBeEquivalentTo(file);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
		public async void CrystalProcessing_InvalidCif_GenerateExpectedRecordEntity()
		{
            var recordView = Records.Find(new BsonDocument("FileId", FileId)).FirstOrDefault() as IDictionary<string, object>;
            recordView.Should().NotBeNull();

			var recordId = Harness.GetProcessedRecords(FileId).First();
            var record = await Session.Get<Crystal>(recordId);
            recordView.Should().EntityShouldBeEquivalentTo(record);
		}

		[Fact, ProcessingTrait(TraitGroup.All, TraitGroup.Crystal)]
		public void CrystalProcessing_InvalidCif_GenerateExpectedRecordNode()
		{
            var recordNode = Nodes.Find(new BsonDocument("FileId", FileId)).FirstOrDefault() as IDictionary<string, object>;
            recordNode.Should().BeNull();
		}
    }
}
