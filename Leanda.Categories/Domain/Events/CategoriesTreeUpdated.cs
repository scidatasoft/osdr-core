using Leanda.Categories.Domain.ValueObjects;
using Sds.CqrsLite.Events;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Events
{
    public class CategoriesTreeUpdated : IUserEvent
	{
        public Guid? ParentId { get; set; }
        public List<TreeNode> Nodes { get; set; }

        public CategoriesTreeUpdated(Guid id, Guid userId, Guid? parentId, List<TreeNode> nodes)
        {
            Id = id;
            Nodes = nodes;
            UserId = userId;
            ParentId = parentId;
		}

        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public int Version { get; set; }
	}
}
