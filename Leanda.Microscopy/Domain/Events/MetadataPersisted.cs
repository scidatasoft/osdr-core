using System;

namespace Leanda.Microscopy.Domain.Events
{
    public interface MetadataPersisted
    {
        Guid Id { get; }
        Guid UserId { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
