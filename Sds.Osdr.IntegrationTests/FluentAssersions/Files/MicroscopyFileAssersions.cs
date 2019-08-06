using FluentAssertions;
using FluentAssertions.Collections;
using Leanda.Microscopy.Domain;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.Generic.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Sds.Osdr.IntegrationTests.FluentAssersions
{
    public static class MicroscopyFileAssersionsExtensions
    {
        public static void EntityShouldBeEquivalentTo(this GenericDictionaryAssertions<string, object> assertions, MicroscopyFile file)
        {
            assertions.Subject.ShouldAllBeEquivalentTo(new Dictionary<string, object>
            {
                { "_id", file.Id},
                { "Blob", new Dictionary<string, object>() {
                    { "_id", file.BlobId},
                    { "Bucket", file.Bucket },
                    { "Length", file.Length },
                    { "Md5", file.Md5 }
                } },
                { "SubType", FileType.Microscopy.ToString() },
                { "OwnedBy", file.OwnedBy },
                { "CreatedBy", file.CreatedBy },
                { "CreatedDateTime", file.CreatedDateTime.UtcDateTime},
                { "UpdatedBy", file.UpdatedBy },
                { "UpdatedDateTime", file.UpdatedDateTime.UtcDateTime},
                { "ParentId", file.ParentId },
                { "Name", file.FileName },
                { "Status", file.Status.ToString() },
                { "Version", file.Version },
                { "Images", file.Images.Select(i => new Dictionary<string, object> {
                    { "_id", i.Id },
                    { "Bucket", file.Bucket },
                    { "Height", i.Height },
                    { "Width", i.Height },
                    { "MimeType", i.MimeType },
                    { "Scale", i.GetScale() }
                })},
                { "Properties", new Dictionary<string, object>() {
                    { "BioMetadata", file.BioMetadata }
                }}
            });
        }
    }
}
