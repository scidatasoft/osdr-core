using Sds.CqrsLite.Events;
using Sds.Osdr.Domain;
using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Events
{
    public class BioMetadataUpdated : IUserEvent
    {
        public readonly IList<KeyValue<string>> Metadata;

        public BioMetadataUpdated(Guid id, Guid userId, IList<KeyValue<string>> metadata)
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
