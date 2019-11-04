﻿using FluentAssertions;
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
    public class GetAllCategoryTrees : OsdrWebTest
    {

        public GetAllCategoryTrees(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            var categories = new List<TreeNode>()
            {
                new TreeNode(Guid.NewGuid(), "Projects", new List<TreeNode>()
                {
                    new TreeNode(Guid.NewGuid(), "Projects One"),
                    new TreeNode(Guid.NewGuid(), "Projects Two")
                })
            };

            for (int i = 0; i < 10; i++)
            {
                var response = JohnApi.PostData("/api/categories/tree", categories).Result;

                var content = response.Content.ReadAsStringAsync().Result;

                var categoryId = Guid.Parse(content);

                Harness.WaitWhileCategoryTreePersisted(categoryId);
            }
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Folder)]
        public async Task CategoryTreeOperations_GetAllCategoryTrees_ExpectedListOfCategories()
        {
            var response = await JohnApi.GetData($"/api/categories/tree");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonCategories = JArray.Parse(await response.Content.ReadAsStringAsync());
            jsonCategories.Should().NotBeEmpty();
            foreach (var category in jsonCategories.Children())
            {
                category.Should().ContainsJson($@"
                {{
            	    'id': *EXIST*,
            	    'createdBy': '{JohnId}',
            	    'createdDateTime': *EXIST*,
            	    'updatedBy': '{JohnId}',
            	    'updatedDateTime': *EXIST*,
            	    'version': *EXIST*
                }}");
            }

        }
    }
}