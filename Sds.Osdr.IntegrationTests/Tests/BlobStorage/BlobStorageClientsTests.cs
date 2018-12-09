using FluentAssertions;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.IntegrationTests.Tests
{
    public class BlobStorageClientsTestsFixture
    {
        public Guid BlobId { get; set; }

        public BlobStorageClientsTestsFixture(OsdrTestHarness harness)
        {
            using (var blobStorage = new BlobStorageClient())
            {
                blobStorage.AuthorizeClient("osdr_ml_modeler", "osdr_ml_modeler_secret").Wait();

                BlobId = blobStorage.AddResource("CLIENT_ID", "Aspirin.mol", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "SkipOsdrProcessing", true } }).Result;
            }
        }
    }

    [Collection("OSDR Test Harness")]
    public class BlobStorageClientsTests : OsdrTest, IClassFixture<BlobStorageClientsTestsFixture>
    {
        private Guid BlobId { get; set; }

        public BlobStorageClientsTests(OsdrTestHarness fixture, ITestOutputHelper output, BlobStorageClientsTestsFixture initFixture) : base(fixture, output)
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
                await blobStorage.AuthorizeClient("osdr_ml_modeler", "osdr_ml_modeler_secret");

                var info = await blobStorage.GetBlobInfo("CLIENT_ID", BlobId);

                info["fileName"].Should().Be("Aspirin.mol");
            }
        }
    }
}
