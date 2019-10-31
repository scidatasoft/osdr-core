using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class DeleteEntityCategoryFixture
    {
        public Guid RootCategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public DeleteEntityCategoryFixture(OsdrWebTestHarness harness)
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
    public class DeleteEntityCategory : OsdrWebTest, IClassFixture<DeleteEntityCategoryFixture>
    {
        private Guid RootCategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public DeleteEntityCategory(OsdrWebTestHarness harness, ITestOutputHelper output, DeleteEntityCategoryFixture fixture) : base(harness, output)
        {
            RootCategoryId = fixture.RootCategoryId;
            BlobId = fixture.BlobId;
            FileId = fixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task DeleteOneCategoryToEntity()
        {
            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            var fileNodeId = Guid.Parse(fileNode.Value<string>("id"));

            var treeResponse = JohnApi.GetData($"api/categorytrees/tree/{RootCategoryId}").Result;
            var treeContent = treeResponse.Content.ReadAsStringAsync().Result;
            var categoryId1 = JObject.Parse(treeContent)["nodes"][0]["children"][0]["_id"].ToString();
            var categoryId2 = JObject.Parse(treeContent)["nodes"][0]["children"][1]["_id"].ToString();

            // add categories to entity 
            await JohnApi.PostData($"/api/categoryentities/entities/{fileNodeId}/categories", new List<string> { categoryId1, categoryId2 });
            // check if node exists by categoryId1
            var firstCategoryAddedNode = GetNodeByCategoryId(categoryId1);
            firstCategoryAddedNode.Value<string>("_id").Should().Be(fileNodeId.ToString());

            // delete first category from node
            await JohnApi.DeleteData($"/api/categoryentities/entities/{fileNodeId}/categories/{categoryId1}");
            // check if node contains categoryId1
            var firstCategoryDeletedNode = GetNodeByCategoryId(categoryId1);
            firstCategoryDeletedNode.Should().BeNull();
        }

        JObject GetNodeByCategoryId(string categoryId)
        {
            var nodesFromES = new List<JObject>();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                var getResponse = JohnApi.GetData($"/api/categoryentities/categories/{categoryId}").Result;
                var nodesResponseContent = getResponse.Content.ReadAsStringAsync().Result;
                nodesFromES = JsonConvert.DeserializeObject<List<JObject>>(nodesResponseContent);
                if (nodesFromES.Count != 0) return nodesFromES[0];
            }
            return null;
        }
    }
}
