using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
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
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();
            var guidThree = Guid.NewGuid();

            var categories = new List<TreeNode>()
            {
                new TreeNode(guidOne, "Projects", new List<TreeNode>()
                {
                    new TreeNode(guidTwo, "Projects One"),
                    new TreeNode(guidThree, "Projects Two")
                })
            };

            var response = JohnApi.PostData("/api/categories/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            categoryId = Guid.Parse(content);

            Harness.WaitWhileCategoryTreePersisted(categoryId);

            var json = $@"[
              {{
                'id': '{guidOne}',
                'title': 'Level 0: Main Node 1',
                'children': [
                  {{ 'id': '{guidTwo}', 'title': 'Level 1: Node 1', 'children': null }},
                  {{ 'id': '{guidThree}', 'title': 'Level 1: Node 2', 'children': null }},
                  {{ 'id': '77197372-668f-27a8-4d9f-1f8cde3a78c5', 'title': 'Level 1: Node 3', 'children': null }}
                ]
              }},
              {{ 'title': 'NoNameNode' }},
              {{ 'title': '1' }},
              {{ 'title': '2' }},
              {{ 'title': '3' }},
              {{ 'title': '4', 'children': [{{ 'title': '4-1' }}, {{ 'title': '4-2', 'children': [{{ 'title': '4-2-1' }}] }}] }}
            ]";
            categories = JsonConvert.DeserializeObject<List<TreeNode>>(json);
            //    new List<TreeNode>
            //{
            //    new TreeNode(Guid.NewGuid(), "Projects", new List<TreeNode>
            //    {
            //        new TreeNode(Guid.NewGuid(), "One", new List<TreeNode> { new TreeNode(Guid.NewGuid(), "Sub") }),
            //        new TreeNode(Guid.NewGuid(), "Two")
            //    })
            //};

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
    }
} 