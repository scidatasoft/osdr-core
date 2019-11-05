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
    public class SharingMLModelFixture
    {
        public Guid FolderId { get; }

        public SharingMLModelFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "valid one model with success optimization" } }).Result;
        }
    }
    //public class SharingMLModelFixture
    //{
    //    private bool _isInitialized = false;
    //    public Guid FolderId { get; set; }
        
    //    public SharingMLModelFixture()
    //    {
    //    }

    //    public void Initialize(Guid userId, OsdrWebTest fixture)
    //    {
    //        if (!_isInitialized)
    //        {
    //            _isInitialized = true;
                
    //            FolderId = fixture.TrainModel(userId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", userId }, { "case", "valid one model with success optimization" } }, true).Result;
    //        }
    //    }
    //}

    [Collection("OSDR Test Harness")]
    public class SharingMLModel : OsdrWebTest, IClassFixture<SharingMLModelFixture>
    {
        public Guid FolderId { get; }

        public SharingMLModel(SharingMLModelFixture testFixture, OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            FolderId = testFixture.FolderId;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTraining_There_Are_No_Errors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }
        
        [Fact(Skip = "Refactoring needed"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTraining_AllGenericFilesProcessed()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            foreach (var modelId in models)
            {
                var model = await Session.Get<Model>(modelId);
                model.Should().NotBeNull();
                model.Status.Should().Be(ModelStatus.Processed);
                
                var responseSetPublic = JohnApi.SetPublicModelsEntity(modelId, true).GetAwaiter().GetResult();
                Harness.WaitWhileModelShared(modelId);
                responseSetPublic.EnsureSuccessStatusCode();
            }

            var response = await JohnApi.GetPublicNodes();
            var nodesContent = await response.Content.ReadAsStringAsync();
            var nodes = JToken.Parse(nodesContent);
            nodes.Should().HaveCount(1);
            
            await Task.CompletedTask;
        }
        
        [Fact(Skip = "Refactoring needed"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTraining_AllGenericEntities()
        {
            var models = Harness.GetDependentFilesExcept(FolderId, FileType.Image, FileType.Tabular, FileType.Pdf);
            foreach (var modelId in models)
            {
                var model = await Session.Get<Model>(modelId);
                model.Should().NotBeNull();
                model.Status.Should().Be(ModelStatus.Processed);
                
                var responseSetPublic = JohnApi.SetPublicModelsEntity(modelId, true).GetAwaiter().GetResult();
                Harness.WaitWhileModelShared(modelId);
                responseSetPublic.EnsureSuccessStatusCode();
            }

            var response = await JohnApi.GetPublicEntity("models");
            var nodesContent = await response.Content.ReadAsStringAsync();
            var entities = JToken.Parse(nodesContent);
            entities.Should().HaveCount(1);
            
            await Task.CompletedTask;
        }
    }
}