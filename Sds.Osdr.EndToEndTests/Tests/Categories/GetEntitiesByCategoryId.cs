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
    [Collection("OSDR Test Harness")]
    public class GetEntitiesByCategoryId : OsdrWebTest, IClassFixture<GetCategoriesIdsByEntityIdFixture>
    {
        private Guid CategoryId;
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }


        public GetEntitiesByCategoryId(OsdrTestHarness harness, ITestOutputHelper output, GetCategoriesIdsByEntityIdFixture fixture) : base(harness, output)
        {
            CategoryId = fixture.CategoryId;
            BlobId = fixture.BlobId;
            FileId = fixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Categories)]
        public async Task GetCategoriesIdsByEntityIdTest()
        {
            var categoriesIds = await JohnApi.ReadJsonAsync<IEnumerable<string>>($"/api/categoryentities/categories/{CategoryId}");
            categoriesIds.Any(x => x == CategoryId.ToString()).Should().BeTrue();
        }
    }
}
