using Leanda.Categories.Domain.ValueObjects;
using MassTransit;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Commands
{
    public interface UpdateCategoriesTree : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        Guid? ParentId { get; set; }
        List<TreeNode> Nodes { get; set; }
        Guid UserId { get; }
	}
}
