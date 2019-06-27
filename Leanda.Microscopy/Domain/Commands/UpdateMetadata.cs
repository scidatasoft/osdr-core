using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Commands
{
    public interface UpdateMetadata
    {
        IDictionary<string, object> Metadata { get; }
        Guid Id { get; }
        Guid UserId { get; }
        int ExpectedVersion { get; }
    }
}
