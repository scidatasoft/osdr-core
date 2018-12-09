using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.IntegrationTests;
using Sds.Osdr.IntegrationTests.FluentAssersions;
using Sds.Osdr.IntegrationTests.Traits;
using Sds.Osdr.WebApi.IntegrationTests;
using Sds.Osdr.WebApi.IntegrationTests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.Osdr.EndToEndTests
{
    public class ValidSdfProcessingFixture
    {
        public Guid BlobId { get; set; }
        public Guid FileId { get; set; }

        public ValidSdfProcessingFixture(OsdrTestHarness harness)
        {
            BlobId = harness.JohnBlobStorageClient.AddResource(harness.JohnId.ToString(), "drugbank_10_records.sdf", new Dictionary<string, object>() { { "parentId", harness.JohnId } }).Result;

            FileId = harness.WaitWhileRecordsFileProcessed(BlobId);
        }
    }

    [Collection("OSDR Test Harness")]
    public class ValidSdfProcessing : OsdrWebTest, IClassFixture<ValidSdfProcessingFixture>
    {
        private Guid BlobId { get; set; }
        private Guid FileId { get; set; }

        public ValidSdfProcessing(OsdrTestHarness fixture, ITestOutputHelper output, ValidSdfProcessingFixture initFixture) : base(fixture, output)
        {
            BlobId = initFixture.BlobId;
            FileId = initFixture.FileId;
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_GenerateExpectedFileEntity()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var fileEntityResponse = await JohnApi.GetFileEntityById(FileId);
            var fileEntity = JsonConvert.DeserializeObject<JObject>(await fileEntityResponse.Content.ReadAsStringAsync());
            fileEntity.Should().NotBeNull();

            fileEntity.Should().ContainsJson($@"
			{{
				'id': '{FileId}',
				'blob': {{
					'id': '{blobInfo.Id}',
					'bucket': '{JohnId}',
					'length': {blobInfo.Length},
					'md5': '{blobInfo.MD5}'
				}},
				'subType': '{FileType.Records}',
				'ownedBy': '{JohnId}',
				'createdBy': '{JohnId}',
				'createdDateTime': *EXIST*,
				'updatedBy': '{JohnId}',
				'updatedDateTime': *EXIST*,
				'parentId': '{JohnId}',
				'name': '{blobInfo.FileName}',
				'status': '{FileStatus.Processed}',
				'version': 8,
				'totalRecords': 10,
				'properties': {{
					'fields': [
						'DATABASE_ID',
                        'DATABASE_NAME',
                        'SMILES',
                        'INCHI_IDENTIFIER',
                        'INCHI_KEY',
                        'FORMULA',
                        'MOLECULAR_WEIGHT',
                        'EXACT_MASS',
                        'JCHEM_ACCEPTOR_COUNT',
                        'JCHEM_AVERAGE_POLARIZABILITY',
                        'JCHEM_BIOAVAILABILITY',
                        'JCHEM_DONOR_COUNT',
                        'JCHEM_FORMAL_CHARGE',
                        'JCHEM_GHOSE_FILTER',
                        'JCHEM_IUPAC',
                        'ALOGPS_LOGP',
                        'JCHEM_LOGP',
                        'ALOGPS_LOGS',
                        'JCHEM_MDDR_LIKE_RULE',
                        'JCHEM_NUMBER_OF_RINGS',
                        'JCHEM_PHYSIOLOGICAL_CHARGE',
                        'JCHEM_PKA',
                        'JCHEM_PKA_STRONGEST_ACIDIC',
                        'JCHEM_PKA_STRONGEST_BASIC',
                        'JCHEM_POLAR_SURFACE_AREA',
                        'JCHEM_REFRACTIVITY',
                        'JCHEM_ROTATABLE_BOND_COUNT',
                        'JCHEM_RULE_OF_FIVE',
                        'ALOGPS_SOLUBILITY',
                        'JCHEM_TRADITIONAL_IUPAC',
                        'JCHEM_VEBER_RULE',
                        'DRUGBANK_ID',
                        'SECONDARY_ACCESSION_NUMBERS',
                        'DRUG_GROUPS',
                        'GENERIC_NAME',
                        'PRODUCTS',
                        'SALTS',
                        'SYNONYMS',
                        'INTERNATIONAL_BRANDS',
                        'JCHEM_ATOM_COUNT'
					],
					'chemicalProperties': [
						'MOST_ABUNDANT_MASS',
						'MONOISOTOPIC_MASS',
						'MOLECULAR_WEIGHT',
						'MOLECULAR_FORMULA',
						'SMILES',
						'InChIKey',
						'InChI'
					]
				}}
			}}");
            fileEntity["images"].Should().NotBeNull();
            fileEntity["images"].Should().HaveCount(1);
        }

        [Fact, WebApiTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_GenerateExpectedFileNode()
        {
            var blobInfo = await BlobStorage.GetFileInfo(BlobId, JohnId.ToString());
            blobInfo.Should().NotBeNull();

            var fileNodeResponse = await JohnApi.GetNodeById(FileId);
            var fileNode = JsonConvert.DeserializeObject<JObject>(await fileNodeResponse.Content.ReadAsStringAsync());
            fileNode.Should().ContainsJson($@"
			{{
				'id': '{FileId}',
				'type': 'File',
				'subType': 'Records',
				'blob': {{
					'id': '{blobInfo.Id}',
					'bucket': '{JohnId}',
					'length': {blobInfo.Length},
					'md5': '{blobInfo.MD5}'
				}},
				'status': '{FileStatus.Processed}',
				'ownedBy': '{JohnId}',
				'createdBy': '{JohnId}',
				'createdDateTime': *EXIST*,
				'updatedBy':'{JohnId}',
				'updatedDateTime': *EXIST*,
				'name': '{blobInfo.FileName}',
				'parentId': '{JohnId}',
				'version': *EXIST*,
				'totalRecords': 10
			}}");
            fileNode["images"].Should().NotBeNull();
            fileNode["images"].Should().HaveCount(1);
        }

        [Fact(Skip = "Broken"), WebApiTrait(TraitGroup.All, TraitGroup.Chemical)]
        public async Task ChemicalProcessing_ValidSdf_GenerateExpectedRecordNodeAndRecordEntity()
        {
            var recordResponse = await JohnApi.GetNodesById(FileId);
            var recordNodes = JsonConvert.DeserializeObject<JArray>(await recordResponse.Content.ReadAsStringAsync());

            recordNodes.Should().HaveCount(10);
            var recordIndex = 0;

            foreach (var recordNodesItem in recordNodes)
            {
                var recordId = recordNodesItem["id"].ToObject<Guid>();
                recordId.Should().NotBeEmpty();

                await ValidRecordEntity(recordId, recordIndex);
                await ValidRecordNode(recordId, recordIndex);

                recordIndex++;
            }
        }
        private async Task ValidRecordNode(Guid recordId, int recordIndex)
        {
            var recordNodeResponse = await JohnApi.GetNodeById(recordId);
            var recordNode = JsonConvert.DeserializeObject<JObject>(await recordNodeResponse.Content.ReadAsStringAsync());
            recordNode.Should().NotBeEmpty();
            recordNode.Should().ContainsJson($@"
				{{
 					'id': '{recordId}',
					'type': 'Record',
					'subType': 'Structure',
					'name': {recordIndex},
					'blob': {{
						'bucket': '{JohnId}'
					}},
					'ownedBy': '{JohnId}',
					'createdBy': '{JohnId}',
					'createdDateTime': *EXIST*,
					'updatedBy': '{JohnId}',
					'updatedDateTime': *EXIST*,
					'parentId': '{FileId}',
					'version': *EXIST*,
					'status': '{FileStatus.Processed}',
				}}");
            recordNode["images"].Should().NotBeNull();
            recordNode["images"].Should().HaveCount(1);
        }
        private async Task ValidRecordEntity(Guid recordId, int recordIndex)
        {
            var recordEntityResponse = await JohnApi.GetRecordEntityById(recordId);
            var recordEntity = JsonConvert.DeserializeObject<JObject>(await recordEntityResponse.Content.ReadAsStringAsync());
            recordEntity.Should().NotBeEmpty();

            recordEntity.Should().ContainsJson($@"
				{{
					'id': '{recordId}',
					'type': 'Structure',
					'fileId': '{FileId}',
					'blob': {{
						'bucket': '{JohnId}',
					}},
					'ownedBy': '{JohnId}',
					'createdBy': '{JohnId}',
					'createdDateTime': *EXIST*,
					'updatedBy': '{JohnId}',
					'updatedDateTime': *EXIST*,
					'index': {recordIndex},
					'status': '{FileStatus.Processed}',
					'version': *EXIST*,
					'properties': {{
						'fields': [
							{{ 
								'name': 'StdInChI', 
								'value': 'StdInChI-{recordIndex}'
							}},
							{{
								'name': 'StdInChIKey',
								'value': 'StdInChIKey-{recordIndex}'
							}},
							{{
								'name': 'SMILES',
								'value': 'SMILES-{recordIndex}'
							}}
						],
						'chemicalProperties': *EXIST*
					}}	
				}}");
            recordEntity["images"].Should().NotBeNull();
            recordEntity["images"].Should().HaveCount(1);
        }
    }
}