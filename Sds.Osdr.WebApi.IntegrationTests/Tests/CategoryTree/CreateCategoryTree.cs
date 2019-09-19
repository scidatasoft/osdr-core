using Leanda.Categories.Domain.ValueObjects;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    [Collection("OSDR Test Harness")]
    public class CreateCategoryTree : OsdrWebTest
    {
        private Guid categoryId;

        public CreateCategoryTree(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode(categoryId, "Projects", new List<TreeNode>()
                {
                    new TreeNode(Guid.NewGuid(), "Projects One"),
                    new TreeNode(Guid.NewGuid(), "Projects Two")
                })
            };

            var response = JohnApi.PostData("/api/categories", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            Guid.TryParse(content, out categoryId);

            Harness.WaitWhileCategoryTreePersisted(categoryId);
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_CreateNewCategoryTree_ExpectedCreatedFolder()
        {
            var response = await JohnApi.GetData($"/api/categories/{categoryId}/tree");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            //var jsonFolder = JToken.Parse(await response.Content.ReadAsStringAsync());

            //      jsonFolder.Should().ContainsJson($@"
            //{{
            //	'id': '{_folderId}',
            //	'createdBy': '{JohnId}',
            //	'createdDateTime': *EXIST*,
            //	'updatedBy': '{JohnId}',
            //	'updatedDateTime': *EXIST*,
            //	'ownedBy': '{JohnId}',
            //	'name': 'new folder',
            //	'status': 'Created',
            //	'version': 1
            //}}");
        }
    }
}