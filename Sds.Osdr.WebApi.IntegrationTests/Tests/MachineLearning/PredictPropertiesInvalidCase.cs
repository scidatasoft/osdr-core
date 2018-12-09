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
    public class PredictPropertiesInvalidCaseFixture
    {
        public Guid FolderId { get; set; }

        public PredictPropertiesInvalidCaseFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.PredictProperties("invalid case", "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class PredictPropertiesInvalidCase : OsdrWebTest, IClassFixture<PredictPropertiesInvalidCaseFixture>
    {
        private Guid FolderId { get; set; }

        public PredictPropertiesInvalidCase(OsdrWebTestHarness fixture, ITestOutputHelper output, PredictPropertiesInvalidCaseFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact(Skip = "Unstable"), WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlPrediction_ThereAreNoErrors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

    }
}