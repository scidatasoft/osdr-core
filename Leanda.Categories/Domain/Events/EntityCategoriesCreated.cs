using Leanda.Categories.Domain.ValueObjects;
using Sds.CqrsLite.Events;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Events
{
    public class EntityCategoriesCreated : IUserEvent
	{
        public IEnumerable<Guid> CategoriesIds { get; set; }

        public EntityCategoriesCreated(Guid id, Guid userId, List<Guid> categoriesIds)
        {
            Id = id;
            CategoriesIds = categoriesIds;
            UserId = userId;
		}

        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public int Version { get; set; }
	}
}
