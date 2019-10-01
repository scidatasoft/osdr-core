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
    [Collection("OSDR Test Harness")]
    public class UpdateCategoryTree : OsdrWebTest
    {
        private Guid categoryId;

        public UpdateCategoryTree(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        { 
            var categories = new List<TreeNode>()
            {
                new TreeNode("Projects", new List<TreeNode>()
                {
                    new TreeNode("Projects One"),
                    new TreeNode("Projects Two")
                })
            };

            var response = JohnApi.PostData("/api/categories/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            categoryId = Guid.Parse(content);

            Harness.WaitWhileCategoryTreePersisted(categoryId);

            var nodeIds = GetNodeIdsForCategory(categoryId).Result.ToList();

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

            response = JohnApi.PutData($"/api/categories/tree/{categoryId}", categories).Result;

            Harness.WaitWhileCategoryTreeUpdatedPersisted(categoryId);
        }


        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_UpdateCategoryTree_ExpectedUpdatedCategory()
        {
            var response = await JohnApi.GetData($"/api/categories/tree/{categoryId}");
            response.EnsureSuccessStatusCode();

            var jsonCategory = JToken.Parse(await response.Content.ReadAsStringAsync());

            jsonCategory.Should().ContainsJson($@"
            {{
            	'id': '{categoryId}',
            	'createdBy': '{JohnId}',
            	'createdDateTime': *EXIST*,
            	'updatedBy': '{JohnId}',
            	'updatedDateTime': *EXIST*,
            	'version': 2,
                'nodes': *EXIST*
            }}");
        }

        private async Task<IEnumerable<Guid>> GetNodeIdsForCategory(Guid categoryId)
        {
            var response = await JohnApi.GetData($"/api/categories/tree/{categoryId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var treeJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)["nodes"].ToString();
            var tree = JsonConvert.DeserializeObject<List<TreeNode>>(treeJson);
            return tree.GetNodeIds();
        }
    }
} 