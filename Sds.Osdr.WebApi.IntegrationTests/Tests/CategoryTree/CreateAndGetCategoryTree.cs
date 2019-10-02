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
    public class CreateAndGetCategoryTreeFixture
    {
        public Guid CategoryId;

        public CreateAndGetCategoryTreeFixture(OsdrWebTestHarness harness)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode("Projects", new List<TreeNode>()
                {
                    new TreeNode("Projects One"),
                    new TreeNode("Projects Two")
                })
            };

            var response = harness.JohnApi.PostData("/api/categories/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            CategoryId = Guid.Parse(content);

            harness.WaitWhileCategoryTreePersisted(CategoryId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class CreateAndGetCategoryTree : OsdrWebTest, IClassFixture<CreateAndGetCategoryTreeFixture>
    {
        private Guid CategoryId;

        public CreateAndGetCategoryTree(OsdrWebTestHarness harness, ITestOutputHelper output, CreateAndGetCategoryTreeFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTree_CreateNewCategoryTree_BuiltExpectedDocument()
        {
            var response = await JohnApi.GetData($"/api/categories/tree/{CategoryId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonCategory = JToken.Parse(await response.Content.ReadAsStringAsync());
             
            jsonCategory.Should().ContainsJson($@"
            {{
            	'id': '{CategoryId}',
            	'createdBy': '{JohnId}',
            	'createdDateTime': *EXIST*,
            	'updatedBy': '{JohnId}',
            	'updatedDateTime': *EXIST*,
            	'version': 1,
                'nodes': *EXIST*
            }}");
        }
    }
}