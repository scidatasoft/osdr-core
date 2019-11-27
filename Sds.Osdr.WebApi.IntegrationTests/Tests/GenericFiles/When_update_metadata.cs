﻿using FluentAssertions;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.WebApi.IntegrationTests.GenericFiles
{
    [Collection("OSDR Test Harness")]
    public class UpdateMetadata : OsdrWebTest
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public UpdateMetadata(OsdrWebTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
		{
            BlobId = fixture.JohnBlobStorageClient.AddResource(fixture.JohnId.ToString(), "2018-02-14.gif", new Dictionary<string, object>() { { "parentId", fixture.JohnId } }).Result;

            FileId = fixture.WaitWhileFileProcessed(BlobId);
        }

        [Fact, WebApiTrait(TraitGroup.All)]
		public async Task UpdateMetadata_UpdateGenericMetadata_ExpectedRenamedFolder()
		{
            var processedFile = await Harness.Session.Get<File>(FileId);

            var url = $"/api/entities/files/{FileId}?version={processedFile.Version}";

            var data = $"[{{'op':'replace','path':'Metadata','value':[{{'name':'test1', 'value': 'value1'}}]}}]";

            JohnApi.PatchData(url, data).Wait();

            Harness.WaitMetadataUpdated(FileId);

            var response = await JohnApi.GetFileEntityById(FileId);
            response.EnsureSuccessStatusCode();
            var json = JToken.Parse(await response.Content.ReadAsStringAsync());

            json.Should().ContainsJson($@"
            {{
            	'id': '{FileId}',
            	'createdBy': '{JohnId}',
            	'createdDateTime': *EXIST*,
            	'updatedBy': '{JohnId}',
            	'updatedDateTime': *EXIST*,
            	'ownedBy': '{JohnId}',
            	'version': 8,
                'properties': {{
                    'metadata':
                    [
                        {{ 
                            'name': 'test1',
                            'value': 'value1'
                        }}
                    ]
                }}
            }}");
        }
    }
}
