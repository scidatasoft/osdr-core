using Leanda.Categories.Domain.ValueObjects;
using Sds.CqrsLite.Events;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Events
{
    public class CategoriesTreeCreated : IUserEvent
	{
        public List<TreeNode> Nodes { get; set; }

        public CategoriesTreeCreated(Guid id, Guid userId, List<TreeNode> nodes)
        {
            Id = id;
            Nodes = nodes;
            UserId = userId;
		}

        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public int Version { get; set; }
	}
}
