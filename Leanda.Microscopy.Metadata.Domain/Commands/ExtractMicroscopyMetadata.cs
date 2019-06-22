using MassTransit;
using System;

namespace Leanda.Microscopy.Metadata.Domain.Commands
{
    public interface ExtractMicroscopyMetadata : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        string Bucket { get; }
        Guid BlobId { get; }
        Guid UserId { get; }
    }
}
