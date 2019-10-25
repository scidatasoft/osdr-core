using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class UpdateCategoryTreeFixture
    {
        public Guid CategoryId;

        public UpdateCategoryTreeFixture(OsdrWebTestHarness harness)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode("Projects", new List<TreeNode>()
                {
                    new TreeNode("Projects One"),
                    new TreeNode("Projects Two")
                })
            };

            var response = harness.JohnApi.PostData("/api/categorytrees/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            CategoryId = Guid.Parse(content);

            harness.WaitWhileCategoryTreePersisted(CategoryId);

            var nodeIds = GetNodeIdsForCategory(harness, CategoryId).Result.ToList();

            var guidOne = nodeIds.ElementAt(0);
            var guidTwo = nodeIds.ElementAt(1);
            var guidThree = nodeIds.ElementAt(2);

            var json = $@"[
              {{
                'id': '{guidOne}',
                'title': 'Level 0: Main Node 1',
                'children': [
                  {{ 'id': '{guidTwo}', 'title': 'Level 1: Node 1', 'children': null }},
                  {{ 'id': '{guidThree}', 'title': 'Level 1: Node 2', 'children': null }}
                ]
              }},
              {{ 'title': 'NoNameNode' }},
              {{ 'title': '1' }},
              {{ 'title': '2' }},
              {{ 'title': '3' }},
              {{ 'title': '4', 'children': [{{ 'title': '4-1' }}, {{ 'title': '4-2', 'children': [{{ 'title': '4-2-1' }}] }}] }}
            ]";
            categories = JsonConvert.DeserializeObject<List<TreeNode>>(json);

            response = harness.JohnApi.PutData($"/api/categorytrees/tree/{CategoryId}", categories).Result;

            harness.WaitWhileCategoryTreeUpdatedPersisted(CategoryId);
        }

        private async Task<IEnumerable<Guid>> GetNodeIdsForCategory(OsdrWebTestHarness harness, Guid categoryId)
        {
            var response = await harness.JohnApi.GetData($"/api/categorytrees/tree/{categoryId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            json = json.Replace("_id", "id");
            var treeJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)["nodes"].ToString();
            var tree = JsonConvert.DeserializeObject<List<TreeNode>>(treeJson);
            return tree.GetNodeIds();
        }
    }

    [Collection("OSDR Test Harness")]
    public class UpdateCategoryTree : OsdrWebTest, IClassFixture<UpdateCategoryTreeFixture>
    {
        private Guid CategoryId;

        public UpdateCategoryTree(OsdrWebTestHarness harness, ITestOutputHelper output, UpdateCategoryTreeFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
        }


        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTree_UpdateCategoryTree_BuiltExpectedDocument()
        {
            var response = await JohnApi.GetData($"/api/categorytrees/tree/{CategoryId}");
            response.EnsureSuccessStatusCode();

            var jsonCategory = JToken.Parse(await response.Content.ReadAsStringAsync());

            jsonCategory.Should().ContainsJson($@"
            {{
            	'_id': '{CategoryId}',
            	'createdBy': '{JohnId}',
            	'createdDateTime': *EXIST*,
            	'updatedBy': '{JohnId}',
            	'updatedDateTime': *EXIST*,
            	'version': 2,
                'nodes': *EXIST*
            }}");
        }
    }
} 