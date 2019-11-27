﻿using Sds.Osdr.Generic.Domain;
using Leanda.Microscopy.Domain.Events;
using System;
using System.Collections.Generic;
using Sds.Osdr.Domain;

namespace Leanda.Microscopy.Domain
{
    public class MicroscopyFile : File
    {
        public IList<KeyValue<string>> BioMetadata { get; protected set; } = new List<KeyValue<string>>();

        private void Apply(MicroscopyFileCreated e)
        {
        }

        private void Apply(BioMetadataUpdated e)
        {
            BioMetadata = e.Metadata;

            UpdatedBy = e.UserId;
            UpdatedDateTime = e.TimeStamp;
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

        public void UpdateBioMetadata(Guid userId, IList<KeyValue<string>> metadata)
        {
            ApplyChange(new BioMetadataUpdated(Id, userId, metadata));
        }
    }
}
