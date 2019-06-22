using System;

namespace Leanda.Microscopy.Sagas.Commands
{
    public interface ProcessMicroscopyFile
    {
        Guid Id { get; }
        Guid ParentId { get; }
        string Bucket { get; }
        Guid BlobId { get; }
        Guid UserId { get; }
    }
}
