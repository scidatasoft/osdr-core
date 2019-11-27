using Leanda.Microscopy.Metadata.Domain.Commands;
using Leanda.Microscopy.Metadata.Domain.Events;
using MassTransit;
using Sds.Storage.Blob.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public class MicroscopyMetadata : IConsumer<ExtractMicroscopyMetadata>
    {
        private readonly IBlobStorage _blobStorage;

        public MicroscopyMetadata(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage)); ;
        }

        public async Task Consume(ConsumeContext<ExtractMicroscopyMetadata> context)
        {
            await context.Publish<MicroscopyMetadataExtracted>(new
            {
                Id = context.Message.Id,
                CorrelationId = context.Message.CorrelationId,
                UserId = context.Message.UserId,
                TimeStamp = DateTimeOffset.UtcNow,
                Metadata = new Dictionary<string, object>()
                {
                    { "Experimenter", "Experimenter name" },
                    { "Experimenter Group", "Experimenter group name" },
                    { "Project", "Project name" },
                    { "Experiment", "Experiment" }
                }
            });
        }
    }
}
