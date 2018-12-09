using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.MachineLearning.Domain;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class TrainOneModelAndFailDuringTheReportGenerationFixture
    {
        public Guid FolderId { get; set; }

        public TrainOneModelAndFailDuringTheReportGenerationFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "drugbank_10_records.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "train one model and fail during the report generation" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class TrainOneModelAndFailDuringTheReportGeneration : OsdrWebTest, IClassFixture<TrainOneModelAndFailDuringTheReportGenerationFixture>
    {
        private Guid FolderId { get; set; }
        
        public TrainOneModelAndFailDuringTheReportGeneration(OsdrWebTestHarness fixture, ITestOutputHelper output, TrainOneModelAndFailDuringTheReportGenerationFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact(Skip = "Unstable"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_TrainOneModelAndFailDuringTheReportGeneration_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact(Skip ="Unstable"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_TrainOneModelAndFailDuringTheReportGeneration_WaitsWhileAllAssociatedGenericFilesProcessed()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            models.Should().HaveCount(1);

            foreach (var modelId in models)
            {
                var model = await Session.Get<Model>(modelId);
                model.Should().NotBeNull();
                model.Status.Should().Be(ModelStatus.Processed);
                model.Images.Should().HaveCount(3);
            }

            var files = (Harness.GetDependentFiles(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf)).ToList();
            files.Should().HaveCount(2);
            files.ToList().ForEach(async fileId =>
            {
                var fileResponse = await JohnApi.GetFileEntityById(fileId);
                fileResponse.EnsureSuccessStatusCode();

                var jsonFile = JToken.Parse(await fileResponse.Content.ReadAsStringAsync());
                jsonFile["status"].Should().BeEquivalentTo("Processed");
            });

            await Task.CompletedTask;


            //var files = Fixture.GetDependentFiles(FolderId).ToList();
            //files.Should().HaveCount(5);

            //files.ForEach(async id =>
            //{
            //    var fileResponse = await Api.GetFileEntityById(id);
            //    fileResponse.EnsureSuccessStatusCode();

            //    var jsonFile = JToken.Parse(await fileResponse.Content.ReadAsStringAsync());
            //    jsonFile["status"].Should().BeEquivalentTo("Processed");
            //});

            //await Task.CompletedTask;
        }
    }
}