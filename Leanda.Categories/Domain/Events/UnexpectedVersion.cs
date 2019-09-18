using System;

namespace Leanda.Categories.Domain.Events
{
    public interface UnexpectedVersion
    {
        Guid Id { get; }
        Guid UserId { get; }
        int Version { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
