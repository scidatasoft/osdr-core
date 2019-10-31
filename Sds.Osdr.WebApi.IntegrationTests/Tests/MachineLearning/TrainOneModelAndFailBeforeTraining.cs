using FluentAssertions;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class TrainOneModelAndFailBeforeTrainingFixture
    {
        public Guid FolderId { get; set; }

        public TrainOneModelAndFailBeforeTrainingFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "drugbank_10_records.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "fail before starting training" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class TrainOneModelAndFailBeforeTraining : OsdrWebTest, IClassFixture<TrainOneModelAndFailBeforeTrainingFixture>
    {
        private Guid FolderId { get; set; }
        
        public TrainOneModelAndFailBeforeTraining(OsdrWebTestHarness fixture, ITestOutputHelper output, TrainOneModelAndFailBeforeTrainingFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact(Skip = "Ignore"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_InvalidModelTraining_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_InvalidModelTraining_DidNotGenerateAnyGenericFiles()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            models.Should().HaveCount(1);

            var files = Harness.GetDependentFiles(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            files.Should().HaveCount(0);

            await Task.CompletedTask;
        }
    }
}