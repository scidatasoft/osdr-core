using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class TrainOneValidModelReverseEventsFixture
    {
        public Guid FolderId { get; set; }

        public TrainOneValidModelReverseEventsFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "valid one model (reverse events order)" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class TrainOneValidModelReverseEvents : OsdrWebTest, IClassFixture<TrainOneValidModelReverseEventsFixture>
    {
        private Guid FolderId { get; set; }

        public TrainOneValidModelReverseEvents(OsdrWebTestHarness fixture, ITestOutputHelper output, TrainOneValidModelReverseEventsFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact(Skip = "Ignore"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ReverseEvnetsOrder_ThereAreNoErrors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTraining_WaitsWhileAllAssociatedGenericFilesProcessed()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            models.Should().HaveCount(1);

            foreach (var modelId in models)
            {
                var modelGenericFiles = Harness.GetDependentFiles(modelId, FileType.Image, FileType.Tabular, FileType.Pdf);
                modelGenericFiles.Should().HaveCount(5);

                modelGenericFiles.ToList().ForEach(async fileId =>
                {
                    var file = await Session.Get<File>(modelId);
                    file.Should().NotBeNull();
                    file.Status.Should().Be(FileStatus.Processed);
                });
            }

            var reportFiles = Harness.GetDependentFiles(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            reportFiles.Should().HaveCount(3);

            reportFiles.ToList().ForEach(async id =>
            {
                var fileResponse = await JohnApi.GetFileEntityById(id);
                fileResponse.EnsureSuccessStatusCode();

                var jsonFile = JToken.Parse(await fileResponse.Content.ReadAsStringAsync());
                jsonFile["status"].Should().BeEquivalentTo("Processed");
            });

            await Task.CompletedTask;
        }
    }
}