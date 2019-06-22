using MassTransit;
using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Metadata.Domain.Events
{
    public interface MicroscopyMetadataExtracted : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        Guid UserId { get; }
        DateTimeOffset TimeStamp { get; }
        IDictionary<string, object> Metadata { get; }
    }
}
