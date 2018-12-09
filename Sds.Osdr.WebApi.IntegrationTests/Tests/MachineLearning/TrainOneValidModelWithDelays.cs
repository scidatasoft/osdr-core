using FluentAssertions;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class TrainOneValidModelWithDelaysFixture
    {
        public Guid FolderId { get; set; }

        public TrainOneValidModelWithDelaysFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.TrainModel(harness.JohnId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId }, { "case", "valid one model (with delays)" } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class TrainOneValidModelWithDelays : OsdrWebTest, IClassFixture<TrainOneValidModelWithDelaysFixture>
    {
        private Guid FolderId { get; set; }
        
        public TrainOneValidModelWithDelays(OsdrWebTestHarness fixture, ITestOutputHelper output, TrainOneValidModelWithDelaysFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlProcessing_ModelTrainingWithDelays_ThereAreNoErrors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }
    }
}