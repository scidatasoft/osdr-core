using Leanda.Categories.Domain.ValueObjects;
using Sds.CqrsLite.Events;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Events
{
    public class CategoryTreeDeleted : IUserEvent
	{
        public Guid? NodeId { get; set; }

        public CategoryTreeDeleted(Guid id, Guid userId, Guid? nodeId)
        {
            Id = id;
            UserId = userId;
		}

        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public int Version { get; set; }
	}
}
