using Sds.CqrsLite.Events;
using Sds.Osdr.Domain;
using System;
using System.Collections.Generic;

namespace Sds.Osdr.Generic.Domain.Events.Files
{
    public class MetadataUpdated : IUserEvent
    {
        public readonly IEnumerable<KeyValue<string>> Metadata;

        public MetadataUpdated(Guid id, Guid userId, IEnumerable<KeyValue<string>> metadata)
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
