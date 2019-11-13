using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.EndToEndTests.Tests.Categories
{
    public class CreateCategoryFixture
    {
        public Guid CategoryId;

        public CreateCategoryFixture(OsdrTestHarness harness)
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
        }
    }

    [Collection("OSDR Test Harness")]
    public class CreateCategory : OsdrWebTest, IClassFixture<CreateCategoryFixture>
    {
        private Guid CategoryId;

        public CreateCategory(OsdrTestHarness harness, ITestOutputHelper output, CreateCategoryFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Categories)]
        public async Task CategoryTree_CreateNewCategoryTree_BuiltExpectedDocument()
        {
            var response = await JohnApi.GetData($"/api/categorytrees/tree/{CategoryId}");
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
