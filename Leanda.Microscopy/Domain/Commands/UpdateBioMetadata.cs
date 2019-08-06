using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Commands
{
    public interface UpdateBioMetadata
    {
        IDictionary<string, object> Metadata { get; }
        Guid Id { get; }
        Guid UserId { get; }
        int ExpectedVersion { get; }
    }
}
