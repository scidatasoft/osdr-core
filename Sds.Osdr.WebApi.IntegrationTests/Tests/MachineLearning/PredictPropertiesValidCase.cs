using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
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
    public class PredictPropertiesValidCaseFixture
    {
        public Guid FolderId { get; set; }

        public PredictPropertiesValidCaseFixture(OsdrWebTestHarness harness)
        {
            FolderId = harness.PredictProperties(harness.JohnId.ToString(), "combined lysomotrophic.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;
        }
    }

    [Collection("OSDR Test Harness")]
    public class PredictPropertiesValidCase : OsdrWebTest, IClassFixture<PredictPropertiesValidCaseFixture>
    {
        private Guid FolderId { get; set; }

        public PredictPropertiesValidCase(OsdrWebTestHarness fixture, ITestOutputHelper output, PredictPropertiesValidCaseFixture initFixture) 
            : base(fixture, output)
        {
            FolderId = initFixture.FolderId;
        }
        
        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlPrediction_ThereAreNoErrors()
        {
            Harness.GetFaults().Should().BeEmpty();

            await Task.CompletedTask;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlPrediction_SingleCsvFileProcessed()
        {
            var tabulars = Harness.GetDependentFiles(FolderId, FileType.Tabular);
            tabulars.Should().NotBeNullOrEmpty();
            tabulars.Should().HaveCount(1);

            foreach (var tabularId in tabulars)
            {
                var tabularResponse = await JohnApi.GetFileEntityById(tabularId);
                var tabularJson = JToken.Parse(await tabularResponse.Content.ReadAsStringAsync());
                
                tabularJson.Should().ContainsJson($@"
                {{
                    'id': '{tabularId}',
                    'blob': *EXIST*,
                    'subType': 'Tabular',
                    'ownedBy': '{JohnId}',
                    'createdBy': '{JohnId}',
                    'createdDateTime': *EXIST*,
                    'updatedBy': '{JohnId}',
                    'updatedDateTime': *EXIST*,
                    'parentId': '{FolderId}',
                    'name': 'FocusSynthesis_InStock.csv',
                    'status': 'Processed',
                    'version': 7
                }}");
            }

            await Task.CompletedTask;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.MachineLearning)]
        public async Task MlPrediction_PredictionCsvFilePersisted()
        {
            var tabulars = Harness.GetDependentFiles(FolderId, FileType.Tabular);
            var list = tabulars.ToList();
            tabulars.Should().NotBeNullOrEmpty();
            tabulars.Should().HaveCount(1);

            foreach (var tabularId in tabulars)
            {
                var tabularResponse = await JohnApi.GetNodeById(tabularId);
                var tabularJson = JToken.Parse(await tabularResponse.Content.ReadAsStringAsync());
                
                tabularJson.Should().ContainsJson($@"
                {{
                    'id': '{tabularId}',
                    'blob': *EXIST*,
                    'subType': 'Tabular',
                    'ownedBy': '{JohnId}',
                    'createdBy': '{JohnId}',
                    'createdDateTime': *EXIST*,
                    'updatedBy': '{JohnId}',
                    'updatedDateTime': *EXIST*,
                    'parentId': '{FolderId}',
                    'name': 'FocusSynthesis_InStock.csv',
                    'status': 'Processed',
                    'version': 7
                }}");
            }

            await Task.CompletedTask;
        }
    }
}