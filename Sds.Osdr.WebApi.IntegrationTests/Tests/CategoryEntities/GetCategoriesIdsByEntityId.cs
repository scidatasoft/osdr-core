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
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests
{
    public class GetCategoriesIdsByEntityIdFixture
    {
        public Guid CategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public GetCategoriesIdsByEntityIdFixture(OsdrWebTestHarness harness)
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
    public class GetCategoriesIdsByEntityId : OsdrWebTest, IClassFixture<GetCategoriesIdsByEntityIdFixture>
    {
        private Guid CategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public GetCategoriesIdsByEntityId(OsdrWebTestHarness harness, ITestOutputHelper output, GetCategoriesIdsByEntityIdFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
            BlobId = fixture.BlobId;
            FileId = fixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Categories)]
        public async Task GetCategoriesIdsByEntityIdTest()
        {
            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            var fileNodeId = Guid.Parse(fileNode.Value<string>("id"));

            await JohnApi.PostData($"/api/categoryentities/entities/{fileNodeId}/categories", new List<Guid> { CategoryId });
            WebFixture.WaitWhileCategoryIndexed(fileNodeId.ToString());

            var response = JohnApi.GetData($"/api/categoryentities/entities/{fileNodeId}/categories").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var categoriesIds = JsonConvert.DeserializeObject<IEnumerable<string>>(content);
            categoriesIds.Any(x => x == CategoryId.ToString()).Should().BeTrue();
        }
    }
}