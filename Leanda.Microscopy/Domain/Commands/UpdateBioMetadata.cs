using Sds.Osdr.Domain;
using System;
using System.Collections.Generic;

namespace Leanda.Microscopy.Domain.Commands
{
    public interface UpdateBioMetadata
    {
        IList<KeyValue<string>> Metadata { get; }
        Guid Id { get; }
        Guid UserId { get; }
        int ExpectedVersion { get; }
    }
}
