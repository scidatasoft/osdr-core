using MassTransit;
using System;

namespace Leanda.Microscopy.Sagas.Events
{
    public interface MycroscopyFileProcessed : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        Guid BlobId { get; }
        string Bucket { get; }
        long ProcessedRecords { get; }
        long FailedRecords { get; }
        DateTimeOffset TimeStamp { get; }
    }
}
