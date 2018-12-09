using MassTransit;
using Sds.Osdr.MachineLearning.Domain.Commands;
using Sds.Osdr.MachineLearning.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning : IConsumer<GenerateReport>
    {
        public async Task Consume(ConsumeContext<GenerateReport> context)
        {
            var message = context.Message;
            var correlationId = message.CorrelationId;
            var folderId = message.ParentId;
            var bucket = message.Models.First().Bucket;
            var userId = message.UserId;
            var modelBlobId = message.Models.First().BlobId;

            var blobInfo = await BlobStorage.GetFileInfo(message.SourceBlobId, message.SourceBucket);
            if (blobInfo.Metadata["case"].Equals("train one model and fail during the report generation"))
            {
                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", folderId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ML_report.pdf", new Dictionary<string, object>() { { "parentId", folderId }, { "userId", userId } });

                await context.Publish<ReportGenerationFailed>(new
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId,
                    Message = "Something wrong happened during report generation...",
                    NumberOfGenericFiles = 2
                });
            }
            else
            {
                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", folderId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ML_report.pdf", new Dictionary<string, object>() { { "parentId", folderId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", folderId }, { "userId", userId } });

                await context.Publish<ReportGenerated>(new
                {
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    NumberOfGenericFiles = 3
                });
            }
        }

    }
}
