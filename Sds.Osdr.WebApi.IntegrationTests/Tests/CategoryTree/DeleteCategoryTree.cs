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
    public class DeleteCategoryTree : OsdrWebTest
    {
        private Guid categoryId;

        public DeleteCategoryTree(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode(Guid.NewGuid(), "Projects", new List<TreeNode>()
                {
                    new TreeNode(Guid.NewGuid(), "Projects One"),
                    new TreeNode(Guid.NewGuid(), "Projects Two")
                })
            };

            var response = JohnApi.PostData("/api/categories/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            categoryId = Guid.Parse(content);

            Harness.WaitWhileCategoryTreePersisted(categoryId);
        }


        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_DeleteCategoryTree_ExpectedUpdatedCategory()
        {
            var response = await JohnApi.DeleteData($"/api/categories/tree/{categoryId}");
            response.EnsureSuccessStatusCode();
            Harness.WaitWhileCategoryTreeDeletePersisted(categoryId);

            response = await JohnApi.GetData($"/api/categories/tree/{categoryId}");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}