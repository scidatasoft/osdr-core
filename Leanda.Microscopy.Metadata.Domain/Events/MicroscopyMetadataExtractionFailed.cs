using MassTransit;
using System;

namespace Leanda.Microscopy.Metadata.Domain.Events
{
    public interface MicroscopyMetadataExtractionFailed : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        string Message { get; }
        DateTimeOffset TimeStamp { get; }
        Guid UserId { get; }
    }
}
