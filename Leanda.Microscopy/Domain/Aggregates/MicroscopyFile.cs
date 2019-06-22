using Sds.Osdr.Generic.Domain;
using Leanda.Microscopy.Domain.Events;
using System;

namespace Leanda.Microscopy.Domain
{
    public class MicroscopyFile : File
    {
        private void Apply(MicroscopyFileCreated e)
        {
        }

        protected MicroscopyFile()
        {
        }

		public MicroscopyFile(Guid id, Guid userId, Guid? parentId, string fileName, FileStatus fileStatus, string bucket, Guid blobId, long length, string md5)
            : base(id, userId, parentId, fileName, fileStatus, bucket, blobId, length, md5, FileType.Microscopy)
        {
            Id = id;
			ApplyChange(new MicroscopyFileCreated(Id));
		}
    }
}
