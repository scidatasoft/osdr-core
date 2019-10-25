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
    public class AddCategoriesToEntityFixture
    {
        public Guid CategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public AddCategoriesToEntityFixture(OsdrWebTestHarness harness)
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

            CategoryId = Guid.Parse(content);

            harness.WaitWhileCategoryTreePersisted(CategoryId);

            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "Chemical-diagram.png", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class AddCategoriesToEntity : OsdrWebTest, IClassFixture<AddCategoriesToEntityFixture>
    {
        private Guid CategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public AddCategoriesToEntity(OsdrWebTestHarness harness, ITestOutputHelper output, AddCategoriesToEntityFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
            BlobId = fixture.BlobId;
            FileId = fixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task AddOneCategoryToEntity()
        {
            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            var fileNodeId = Guid.Parse(fileNode.Value<string>("id"));

            var response = JohnApi.PostData("/api/categoryentities/entities/" + fileNodeId, new List<Guid> { CategoryId }).Result;


            // Try to Get nodes by category id
            var nodesFromES = new List<JObject>();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                var getResponse = JohnApi.GetData("/api/categoryentities/entities/" + CategoryId).Result;
                var nodesResponseContent = await getResponse.Content.ReadAsStringAsync();
                nodesFromES = JsonConvert.DeserializeObject<List<JObject>>(nodesResponseContent);
                if (nodesFromES.Count == 0) continue;
            }
            nodesFromES.Count.Should().BePositive();
            nodesFromES[0].Value<string>("_id").Should().Be(fileNodeId.ToString());
        }
    }
}