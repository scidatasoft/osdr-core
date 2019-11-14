using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.EndToEndTests.Tests.Categories
{
    public class DeleteEntityCategoryFixture
    {
        public Guid RootCategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public DeleteEntityCategoryFixture(OsdrTestHarness harness)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode("Category Root", new List<TreeNode>()
                {
                    new TreeNode("My Test Category"),
                    new TreeNode("Projects Two")
                })
            };

            var response = harness.JohnApi.PostData("api/categorytrees/tree", categories).Result;

            var content = response.Content.ReadAsStringAsync().Result;

            RootCategoryId = Guid.Parse(content);

            harness.WaitWhileCategoryTreePersisted(RootCategoryId);

            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Chemical-diagram.png", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class DeleteEntityCategoryTest : OsdrWebTest, IClassFixture<DeleteEntityCategoryFixture>
    {
        private Guid RootCategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public DeleteEntityCategoryTest(OsdrTestHarness harness, ITestOutputHelper output, DeleteEntityCategoryFixture fixture) : base(harness, output)
        {
            RootCategoryId = fixture.RootCategoryId;
            BlobId = fixture.BlobId;
            FileId = fixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Categories)]
        public async Task DeleteCategory_DeleteOneCategoryFromEntity_CategoryIdShouldBeRemovedFromEntity()
        {
            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            var fileNodeId = Guid.Parse(fileNode.Value<string>("id"));

            var treeContent = JohnApi.ReadJsonAsync<JObject>($"api/categorytrees/tree/{RootCategoryId}").Result;
            var categoryId1 = treeContent["nodes"][0]["children"][0]["id"].ToString();
            var categoryId2 = treeContent["nodes"][0]["children"][1]["id"].ToString();

            // add categories to entity
            await JohnApi.PostData($"/api/categoryentities/entities/{fileNodeId}/categories", new List<string> { categoryId1, categoryId2 });
            WebFixture.WaitWhileCategoryIndexed(fileNodeId.ToString());

            var firstCategoryAddedNode = GetNodeByCategoryId(categoryId1);
            firstCategoryAddedNode.Value<string>("id").Should().Be(fileNodeId.ToString());

            // delete first category from node
            await JohnApi.DeleteData($"/api/categoryentities/entities/{fileNodeId}/categories/{categoryId1}");
            // check if node contains categoryId
            WebFixture.WaitWhileCategoryDeleted(categoryId1);
            var firstCategoryDeletedNode = GetNodeByCategoryId(categoryId1);
            firstCategoryDeletedNode.Should().BeNull();

            var entityCategoryIds = await JohnApi.ReadJsonAsync<List<string>>($"/api/categoryentities/entities/{fileNodeId}/categories");
            entityCategoryIds.Should().HaveCount(1);
            entityCategoryIds.Single().Should().Be(categoryId2);
        }

        private JObject GetNodeByCategoryId(string categoryId)
        {
            var nodesResponseContent = JohnApi.ReadJsonAsync($"/api/categoryentities/categories/{categoryId}").Result;
            var elasticSearchNodes = JsonConvert.DeserializeObject<List<JObject>>(nodesResponseContent);
            if (elasticSearchNodes.Count != 0)
                return elasticSearchNodes[0];

            return null;
        }
    }
}
