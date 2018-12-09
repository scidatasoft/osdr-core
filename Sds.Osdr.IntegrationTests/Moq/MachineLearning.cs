using Sds.Storage.Blob.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning : OsdrService
    {
        public MachineLearning(IBlobStorage blobStorage)
            : base(blobStorage)
        {
        }

        private async Task<Guid> SaveFileAsync(string bucket, string fileName, IDictionary<string, object> metadata)
        {
            using (var blobStorage = new BlobStorageClient())
            {
                blobStorage.AuthorizeClient("osdr_ml_modeler", "osdr_ml_modeler_secret").Wait();

                return await blobStorage.AddResource(bucket, fileName, metadata);
            }
        }
    }
}