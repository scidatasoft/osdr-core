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
    public class TrainOneModelAndFailDuringTheTrainingFixture
    {
        public Guid FolderId { get; set; }

        public TrainOneModelAndFailDuringTheTrainingFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "drugbank_10_records.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "train one model and fail during the training" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class TrainOneModelAndFailDuringTheTraining : OsdrWebTest, IClassFixture<TrainOneModelAndFailDuringTheTrainingFixture>
    {
        private Guid FolderId { get; set; }
        
        public TrainOneModelAndFailDuringTheTraining(OsdrWebTestHarness fixture, ITestOutputHelper output, TrainOneModelAndFailDuringTheTrainingFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }

        [Fact(Skip = "Unstable"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_InvalidModelTraining_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact(Skip ="Unstable"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_InvalidModelTraining_WaitsWhileAllAssociatedGenericFilesProcessed()
        {
            var modelId = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Pdf, FileType.Tabular);
            modelId.Should().HaveCount(1);
            modelId.SingleOrDefault().Should().NotBeEmpty();

            var dependentFiles = Harness.GetDependentFiles(modelId.Single()).ToList();
            dependentFiles.Should().HaveCount(2);

            dependentFiles.ToList().ForEach(async id =>
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