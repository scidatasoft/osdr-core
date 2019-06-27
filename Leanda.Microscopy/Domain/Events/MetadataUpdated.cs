using Sds.CqrsLite.Events;
using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Events
{
    public class MetadataUpdated : IUserEvent
    {
        public readonly IDictionary<string, object> Metadata;

        public MetadataUpdated(Guid id, Guid userId, IDictionary<string, object> metadata)
        {
            Id = id;
            UserId = userId;
            Metadata = metadata;
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

        public int Version { get; set; }
    }
}
