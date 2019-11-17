using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Net;
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

            var response = JohnApi.PostData("/api/categorytrees/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            categoryId = Guid.Parse(content);

            Harness.WaitWhileCategoryTreePersisted(categoryId);

            response = JohnApi.DeleteData($"/api/categorytrees/tree/{categoryId}").Result;
            response.EnsureSuccessStatusCode();
            Harness.WaitWhileCategoryTreeDeletePersisted(categoryId);
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_DeleteCategoryTree_ExpectedUpdatedCategory()
        {
            var response = await JohnApi.GetData($"/api/categorytrees/tree/{categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            //try delete already deleted tree
            response = await JohnApi.DeleteData($"/api/categorytrees/tree/{categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}