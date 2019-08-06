using System;
using System.Collections.Generic;

namespace Sds.Osdr.Generic.Domain.Commands.Files
{
    public interface UpdateMetadata
    {
        IDictionary<string, string> Metadata { get; }
        Guid Id { get; }
        Guid UserId { get; }
        int ExpectedVersion { get; }
    }
}
