using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
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
            var categories = new List<TreeNode>()
            {
                new TreeNode(Guid.NewGuid(), "Projects", new List<TreeNode>()
                {
                    new TreeNode(Guid.NewGuid(), "Projects One"),
                    new TreeNode(Guid.NewGuid(), "Projects Two")
                })
            };

            var response = JohnApi.PostData("/api/categories", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            categoryId = Guid.Parse(content);

            Harness.WaitWhileCategoryTreePersisted(categoryId);


            categories = new List<TreeNode>
            {
                new TreeNode(Guid.NewGuid(), "Projects", new List<TreeNode>
                {
                    new TreeNode(Guid.NewGuid(), "One", new List<TreeNode> { new TreeNode(Guid.NewGuid(), "Sub") }),
                    new TreeNode(Guid.NewGuid(), "Two")
                })
            };

            response = JohnApi.PutData($"/api/categories/{categoryId}/tree", categories).Result;

            Harness.WaitWhileCategoryTreeUpdatedPersisted(categoryId);
        }


        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_UpdateCategoryTree_ExpectedUpdatedCategory()
        {
            var response = await JohnApi.GetData($"/api/categories/{categoryId}/tree");
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