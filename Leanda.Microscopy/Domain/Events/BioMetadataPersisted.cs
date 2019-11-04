using System;

namespace Leanda.Microscopy.Domain.Events
{
    public interface BioMetadataPersisted
    {
        Guid Id { get; }
        Guid UserId { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
