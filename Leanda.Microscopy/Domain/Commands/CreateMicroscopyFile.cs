using Sds.Domain;
using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Commands
{
    public interface CreateMicroscopyFile
    {
        Guid Id { get; set; }
        Guid FileId { get; }
        string Bucket { get; }
        Guid BlobId { get; }
        long Index { get; }
        IEnumerable<Field> Fields { get; }
        Guid UserId { get; set; }
    }
}
