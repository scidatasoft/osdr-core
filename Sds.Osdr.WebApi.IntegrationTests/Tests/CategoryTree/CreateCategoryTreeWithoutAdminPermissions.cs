using FluentAssertions;
using Leanda.Categories.Domain.ValueObjects;
using Newtonsoft.Json.Linq;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
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
    public class CreateCategoryTreeWithoutAdminPermissions : OsdrWebTest
    {
        public CreateCategoryTreeWithoutAdminPermissions(OsdrWebTestHarness harness, ITestOutputHelper output) : base(harness, output)
        {
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Categories)]
        public async Task CategoryTree_CreateNewCategoryTree_ReturnsForbidden()
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode("Projects", new List<TreeNode>()
                {
                    new TreeNode("Projects One"),
                    new TreeNode("Projects Two")
                })
            };

            var response = await JaneApi.PostData("/api/categories/tree", categories);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}