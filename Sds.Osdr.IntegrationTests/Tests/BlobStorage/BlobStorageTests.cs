using FluentAssertions;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests.Tests
{
    public class BlobStorageTestsFixture
    {
        public Guid BlobId { get; set; }

        public BlobStorageTestsFixture(OsdrTestHarness harness)
        {
            using (var blobStorage = new BlobStorageClient())
            {
                blobStorage.Authorize("john", "qqq123").Wait();

                BlobId = blobStorage.AddResource(harness.JohnId.ToString(), "Aspirin.mol", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;
            }
        }
    }

    [Collection("OSDR Test Harness")]
    public class BlobStorageTests : OsdrTest, IClassFixture<BlobStorageTestsFixture>
    {
        private Guid BlobId { get; set; }

        public BlobStorageTests(OsdrTestHarness fixture, ITestOutputHelper output, BlobStorageTestsFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
        }

        [Fact, ProcessingTrait(TraitGroup.All)]
        public async Task BlobUpload_ExistingFile_ReturnsGuid()
        {
            BlobId.Should().NotBeEmpty();

            await Task.CompletedTask;
        }

        [Fact, ProcessingTrait(TraitGroup.All)]
        public async Task BlobUpload_GetBlobInfo_ReturnsExpectedBlobInfo()
        {
            using (var blobStorage = new BlobStorageClient())
            {
                await blobStorage.Authorize("john", "qqq123");

                var info = await blobStorage.GetBlobInfo(JohnId.ToString(), BlobId);

                info["fileName"].Should().Be("Aspirin.mol");
            }
        }
    }
}
