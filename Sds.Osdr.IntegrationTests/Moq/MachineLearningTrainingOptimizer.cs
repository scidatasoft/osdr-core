using MassTransit;
using Sds.Osdr.MachineLearning.Domain.Commands;
using Sds.Osdr.MachineLearning.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning : IConsumer<OptimizeTraining>
    {
        public async Task Consume(ConsumeContext<OptimizeTraining> context)
        {
            var message = context.Message;
            var correlationId = message.CorrelationId;
            var bucket = message.SourceBucket;
            var userId = message.UserId;

            var blobInfo = await BlobStorage.GetFileInfo(message.SourceBlobId, message.SourceBucket);

            if (blobInfo.Metadata["case"].Equals("valid one model with success optimization"))
            {
                await LoadBlob(context, userId, bucket, "FocusSynthesis_InStock.csv", "application/octet-stream", new Dictionary<string, object>() { { "parentId", context.Message.TargetFolderId }, { "userId", userId } });

                await context.Publish<TrainingOptimized>(new
                {
                    Id = message.Id,
                    CorrelationId = correlationId,
                    UserId = message.UserId,
                    Scaler = "Somebody knows what is Scaler???",
                    SubSampleSize = (decimal)1,
                    TestDataSize = (decimal)0.2,
                    KFold = 4,
                    Fingerprints = new List<IDictionary<string, object>>()
                        {
                            new Dictionary<string, object>()
                            {
                                { "radius", 2 },
                                { "size", 512 },
                                { "type", "ecfp" }
                            }
                        }
                });
            }

            if (blobInfo.Metadata["case"].Equals("train model with failed optimization"))
            {
                await context.Publish<TrainingOptimizationFailed>(new
                {
                    Id = message.Id,
                    CorrelationId = correlationId,
                    UserId = message.UserId,
                    Message = "It`s just test. Nothing personal."
                });
            }
        }

    }
}
