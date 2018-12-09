using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.MachineLearning.Domain;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class SingleStructurePredictionsFixture
    {
        public Guid FolderId { get; set; }

        public SingleStructurePredictionsFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.PredictProperties(harness.JohnId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "valid one model with success optimization" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class SingleStructurePredictions : OsdrWebTest, IClassFixture<SingleStructurePredictionsFixture>
    {
        private Guid FolderId { get; }

        public SingleStructurePredictions(SingleStructurePredictionsFixture testFixture, OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            FolderId = testFixture.FolderId;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Sharing)]
        public async Task MlProcessing_ModelTraining_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTraining_AllGenericFilesProcessed()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            foreach (var modelId in models)
            {
                var model = await Session.Get<Model>(modelId);
                model.Should().NotBeNull();
                model.Status.Should().Be(ModelStatus.Processed);

                await JohnApi.SetPublicModelsEntity(modelId, true);
                Harness.WaitWhileModelShared(modelId);

                var responseSSP = await JohnApi.CreateSingleStructurePridiction(new RunSingleStructurePrediction
                {
                    Format = "SMILES",
                    ModelIds = new List<Guid> { modelId },
                    PropertyName = "name",
                    Structure = "C1C=CC=C1C1C=CC=C1"
                });
                var predictionId = JToken.Parse(await responseSSP.Content.ReadAsStringAsync())["predictionId"].ToObject<Guid>();
                responseSSP.EnsureSuccessStatusCode();

                var responseStatus = await JohnApi.GetPredictionStatus(predictionId);
                
                var status = JToken.Parse(await responseStatus.Content.ReadAsStringAsync());
                status["id"].ToObject<Guid>().ShouldBeEquivalentTo(predictionId);
//                status["status"].ToObject<string>().ShouldAllBeEquivalentTo("CALCULATING");
            }

            await Task.CompletedTask;
        }
    }
}