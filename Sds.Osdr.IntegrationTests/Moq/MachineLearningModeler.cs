using MassTransit;
using Sds.Osdr.MachineLearning.Domain.Commands;
using Sds.Osdr.MachineLearning.Domain.Events;
using Sds.Osdr.MachineLearning.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning: IConsumer<TrainModel>
    {
        public async Task Consume(ConsumeContext<TrainModel> context)
        {
            var message = context.Message;
            var bucket = message.SourceBucket;
            var correlationId = message.CorrelationId;
            var modelSagaCorrelationId = Guid.NewGuid();
            var folderId = message.ParentId;
            var userId = message.UserId;

            var blobInfo = await BlobStorage.GetFileInfo(message.SourceBlobId, bucket);

            if (blobInfo.FileName.ToLower().Equals("combined lysomotrophic.sdf") && ((blobInfo.Metadata["case"].Equals("valid one model") || (blobInfo.Metadata["case"].Equals("two valid models")) || blobInfo.Metadata["case"].Equals("valid one model with success optimization"))))
            {
                //  Success path
                //  1. Training just one model and generate one image and one CSV
                //  2. Generate one image and one CSV assigned to the training process
                //  3. Generate report PDF at the end

                var modelId = message.Id;

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var modelBlobId = await SaveFileAsync(bucket, "Bernoulli_Naive_Bayes_with_isotonic_class_weights.sav", new Dictionary<string, object>()
                    {
                        {"parentId", folderId},
                        { "userId", userId },
                        {"correlationId", correlationId},
                        {"FileType", "MachineLearningModel"},
                        {
                            "ModelInfo", new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                        },
                        {"SkipOsdrProcessing", true}
                    });

                await context.Publish<ModelTrained>(new
                {
                    Id = modelId,
                    BlobId = modelBlobId,
                    Bucket = message.SourceBucket,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    NumberOfGenericFiles = 5,
                    CorrelationId = correlationId,
                    PropertyName = "name",
                    PropertyCategory = "category",
                    PropertyUnits = "units",
                    PropertyDescription = "description",
                    DatasetTitle = "dataset",
                    DatasetDescription = "really, this is dataset",
                    Modi = 0.2,
                    DisplayMethodName = "Naive Bayes",
                    Metadata = new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                });

                var thummbnailBlobId = await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelThumbnailGenerated>(new
                {
                    Id = modelId,
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Bucket = bucket,
                    BlobId = thummbnailBlobId
                });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });
            }
            else if (blobInfo.FileName.ToLower().Equals("combined lysomotrophic.sdf") && blobInfo.Metadata["case"].Equals("valid one model (with delays)"))
            {
                //  Success path
                //  1. Training just one model and generate one image and one CSV
                //  2. Generate one image and one CSV assigned to the training process
                //  3. Generate report PDF at the end

                var modelId = message.Id;

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });

                Thread.Sleep(10);

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var thummbnailBlobId = await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelThumbnailGenerated>(new
                {
                    Id = modelId,
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Bucket = bucket,
                    BlobId = thummbnailBlobId
                });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var modelBlobId = await SaveFileAsync(bucket, "Bernoulli_Naive_Bayes_with_isotonic_class_weights.sav", new Dictionary<string, object>()
                    {
                        {"parentId", message.ParentId},
                        {"userId", userId },
                        {"correlationId", correlationId },
                        {"FileType", "MachineLearningModel"},
                        {
                            "ModelInfo", new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                        },
                        {"SkipOsdrProcessing", true}
                    });

                Thread.Sleep(10);

                await context.Publish<ModelTrained>(new
                {
                    Id = modelId,
                    BlobId = modelBlobId,
                    Bucket = message.SourceBucket,
                    NumberOfGenericFiles = 2,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId,
                    Property = new Property("category", "name", "units", "description"),
                    Dataset = new Dataset("dataset", "really, this is dataset", message.SourceBlobId, message.SourceBucket),
                    Modi = 0.2,
                    DisplayMethodName = "Naive Bayes",
                    Metadata = new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ML_report.pdf", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                Thread.Sleep(10);
            }
            else if (blobInfo.FileName.ToLower().Equals("combined lysomotrophic.sdf") && blobInfo.Metadata["case"].Equals("valid one model (reverse events order)"))
            {
                //  Success path (reverse events order)
                //  1. Training just one model and generate one image and one CSV
                //  2. Generate one image and one CSV assigned to the training process
                //  3. Generate report PDF at the end

                var modelId = message.Id;

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var modelBlobId = await SaveFileAsync(bucket, "Bernoulli_Naive_Bayes_with_isotonic_class_weights.sav", new Dictionary<string, object>()
                    {
                        {"parentId", message.ParentId},
                        {"correlationid", correlationId },
                        {"FileType", "MachineLearningModel"},
                        {"userId", userId },
                        {
                            "ModelInfo", new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                        },
                        {"SkipOsdrProcessing", true}
                    });

                await context.Publish<ModelTrained>(new
                {
                    Id = modelId,
                    BlobId = modelBlobId,
                    Bucket = message.SourceBucket,
                    NumberOfGenericFiles = 5,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId,
                    Property = new Property("category", "name", "units", "description"),
                    Dataset = new Dataset("dataset", "really, this is dataset", message.SourceBlobId, message.SourceBucket),
                    Modi = 0.2,
                    DisplayMethodName = "Naive Bayes",
                    Metadata = new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                });

                var thumbnailBlobId = await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelThumbnailGenerated>(new
                {
                    Id = modelId,
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Bucket = bucket,
                    BlobId = thumbnailBlobId
                });

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });
            }
            else if (blobInfo.FileName.ToLower().Equals("drugbank_10_records.sdf") && blobInfo.Metadata["case"].Equals("train one model and fail during the training"))
            {
                //  Failed path
                //  1. Training just one model, generate one image and one CSV
                //  2. Issue ModelTrainingFailed during model training

                var modelId = message.Id;

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<TrainingFailed>(new
                {
                    Id = modelId,
                    NumberOfGenericFiles = 2,
                    IsModelTrained = false,
                    IsThumbnailGenerated = false,
                    Message = "Something very bad has happened right after the starting...",
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    UserId = userId
                });
            }
            else if (blobInfo.FileName.ToLower().Equals("drugbank_10_records.sdf") && blobInfo.Metadata["case"].Equals("fail before starting training"))
            {
                //  Failed path
                //  1. Issue ModelTrainingFailed right before starting the first model training

                var modelId = message.Id;

                await context.Publish<TrainingFailed>(new
                {
                    Id = modelId,
                    NumberOfGenericFiles = 0,
                    IsModelTrained = false,
                    IsThumbnailGenerated = false,
                    Message = "Something very bad has happened right after the starting...",
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    UserId = userId
                });
            }
            else if (blobInfo.FileName.ToLower().Equals("drugbank_10_records.sdf") && blobInfo.Metadata["case"].Equals("train one model and fail during the report generation"))
            {
                //  Failed path
                //  1. Training just one model, generate one image and one CSV
                //  2. Successfully finish model's training
                //  3. Generate one image and one CSV assigned to the training
                //  4. Issue ModelTrainingFailed during the report generation

                var modelId = message.Id;

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });


                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var modelBlobId = await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>()
                    {
                        {"parentId", message.ParentId},
                        {"correlationid", correlationId },
                        { "userId", userId },
                        {"FileType", "MachineLearningModel"},
                        {
                            "ModelInfo", new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                        },
                        {"SkipOsdrProcessing", true}
                    });


                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var thumbnailBlobId = await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelThumbnailGenerated>(new
                {
                    Id = modelId,
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Bucket = bucket,
                    BlobId = thumbnailBlobId
                });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelTrained>(new
                {
                    Id = modelId,
                    BlobId = modelBlobId,
                    Bucket = message.SourceBucket,
                    NumberOfGenericFiles = 5,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    CorrelationId = correlationId,
                    Property = new Property("category", "name", "units", "description"),
                    Dataset = new Dataset("dataset", "really, this is dataset", message.SourceBlobId, message.SourceBucket),
                    Modi = 0.2,
                    DisplayMethodName = "Naive Bayes",
                    Metadata = new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                });
            }
            else if (blobInfo.FileName.ToLower().Equals("combined lysomotrophic.sdf") &&
                     blobInfo.Metadata["case"].Equals("valid one model (reverse events order)"))
            {
                //  Success path
                //  3. Training just one model and generate one image and one CSV
                //  2. Generate one image and one CSV assigned to the training process
                //  1. Generate report PDF at the end

                var modelId = message.Id;

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ML_report.pdf", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var thummbnailBlobId = await SaveFileAsync(bucket, "ml-training-image.png", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                await context.Publish<ModelThumbnailGenerated>(new
                {
                    Id = folderId,
                    CorrelationId = correlationId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Bucket = bucket,
                    BlobId = thummbnailBlobId
                });

                await SaveFileAsync(bucket, "FocusSynthesis_InStock.csv", new Dictionary<string, object>() { { "parentId", modelId }, { "userId", userId } });

                var modelBlobId = await SaveFileAsync(bucket, "Bernoulli_Naive_Bayes_with_isotonic_class_weights.sav", new Dictionary<string, object>()
                    {
                          {"parentId", message.ParentId},
                          {"correlationid", correlationId },
                          {"FileType", "MachineLearningModel"},
                          {"userId", userId },
                          {
                              "ModelInfo", new Dictionary<string, object>()
                              {
                                  {"ModelName", "Naive Bayes"},
                                  {"SourceBlobId", message.SourceBlobId},
                                  {"Method", message.Method},
                                  {"SourceBucket", userId.ToString()},
                                  {"ClassName", message.ClassName},
                                  {"SubSampleSize", message.SubSampleSize},
                                  {"KFold", message.KFold},
                                  {"Fingerprints", message.Fingerprints}
                              }
                          },
                          {"SkipOsdrProcessing", true}
                    });

                await context.Publish<ModelTrained>(new
                {
                    Id = modelId,
                    ModelBlobId = modelBlobId,
                    ModelBucket = message.SourceBucket,
                    NumberOfGenericFiles = 2,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    Property = new Property("category", "name", "units", "description"),
                    Dataset = new Dataset("dataset", "really, this is dataset", message.SourceBlobId, message.SourceBucket),
                    Modi = 0.2,
                    DisplayMethodName = "Naive Bayes",
                    Metadata = new Dictionary<string, object>()
                            {
                                {"ModelName", "Naive Bayes"},
                                {"SourceBlobId", message.SourceBlobId},
                                {"Method", message.Method},
                                {"SourceBucket", userId.ToString()},
                                {"ClassName", message.ClassName},
                                {"SubSampleSize", message.SubSampleSize},
                                {"KFold", message.KFold},
                                {"Fingerprints", message.Fingerprints}
                            }
                });

                await context.Publish<ModelTrainingStarted>(new
                {
                    Id = modelId,
                    UserId = userId,
                    TimeStamp = DateTimeOffset.UtcNow,
                    ModelName = "Naive Bayes",
                    CorrelationId = correlationId
                });
            }
        }

    }
}
