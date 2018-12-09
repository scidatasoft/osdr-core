using MassTransit;
using Sds.Osdr.MachineLearning.Domain.Commands;
using Sds.Osdr.MachineLearning.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning: IConsumer<PredictProperties>
    {
        public async Task Consume(ConsumeContext<PredictProperties> context)
        {
            switch (context.Message.DatasetBucket)
            {
                case "failed_case":
                    {
                        await context.Publish<PropertiesPredictionFailed>(new
                        {
                            Message = "It`s business, nothing personal.",
                            Id = context.Message.ParentId,
                            UserId = context.Message.UserId,
                            TimeStamp = DateTimeOffset.UtcNow,
                            CorrelationId = context.Message.CorrelationId
                        });

                        break;
                    }
                default:
                    var predictionsBlobId = await SaveFileAsync(context.Message.DatasetBucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", context.Message.ParentId }, { "userId", context.Message.UserId } });
                    
                    await context.Publish<PropertiesPredicted>(new
                    {
                        Id = predictionsBlobId,
                        FileBucket = context.Message.DatasetBucket,
                        FileBlobId = predictionsBlobId,
                        UserId = context.Message.UserId,
                        TimeStamp = DateTimeOffset.UtcNow,
                        CorrelationId = context.Message.CorrelationId
                    });

                    break;
            }
        }
    }
}
