using MassTransit;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain.Commands
{
    public interface AddEntityCategories : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        IEnumerable<Guid> CategoriesIds { get; set; }
        Guid UserId { get; }
	}
}
