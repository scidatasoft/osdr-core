using Automatonymous;
using Leanda.Microscopy.Domain.Commands;
using Leanda.Microscopy.Domain.Events;
using Leanda.Microscopy.Metadata.Domain.Commands;
using Leanda.Microscopy.Metadata.Domain.Events;
using Leanda.Microscopy.Sagas.Commands;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using Sds.Imaging.Domain.Commands;
using Sds.Imaging.Domain.Events;
using Sds.Osdr.Domain;
using Sds.Osdr.Generic.Domain;
using Sds.Osdr.Generic.Domain.Commands.Files;
using Sds.Osdr.Generic.Domain.Events.Files;
using Sds.Osdr.Generic.Domain.ValueObjects;
using Sds.Osdr.Generic.Sagas.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodeStatusPersisted = Sds.Osdr.Generic.Domain.Events.Files.NodeStatusPersisted;
using StatusPersisted = Sds.Osdr.Generic.Domain.Events.Files.StatusPersisted;

namespace Leanda.Microscopy.Sagas
{
    public class MicroscopyFileProcessingState : SagaStateMachineInstance, IVersionedSaga
    {
        public Guid FileId { get; set; }
        public Guid ParentId { get; set; }
        public Guid UserId { get; set; }
        public Guid BlobId { get; set; }
        public string Bucket { get; set; }
        public string CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }
        public Guid _id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public IList<Guid> Images { get; set; } = new List<Guid>();
        public int AllPersisted { get; set; }
        public int EndProcessing { get; set; }
    }

    public static partial class PublishEndpointExtensions
    {
        public static async Task GenerateImage(this IPublishEndpoint endpoint, MicroscopyFileProcessingState state, int width, int height)
        {
            await endpoint.Publish<GenerateImage>(new
            {
                Id = state.FileId,
                UserId = state.UserId,
                BlobId = state.BlobId,
                Bucket = state.Bucket,
                CorrelationId = state.CorrelationId,
                Image = new Sds.Imaging.Domain.Models.Image()
                {
                    Id = NewId.NextGuid(),
                    Width = width,
                    Height = height,
                    Format = "PNG",
                    MimeType = "image/png"
                }
            });
        }
    }

    public class MicroscopyFileProcessingStateMachine : MassTransitStateMachine<MicroscopyFileProcessingState>
    {
        public MicroscopyFileProcessingStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => ProcessFile, x => x.CorrelateById(context => context.Message.Id).SelectId(context => context.Message.Id));
            Event(() => ImageGenerated, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ImageGenerationFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ImageAdded, x => x.CorrelateById(context => context.Message.Id));
            Event(() => MetadataExtracted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => MetadataExtractionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => MetadataPersisted, x => x.CorrelateById(context => context.Message.Id));
            Event(() => FileProcessed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => StatusChanged, x => x.CorrelateById(context => context.Message.Id));
            Event(() => NodeStatusPersisted, x => x.CorrelateById(context => context.Message.Id));
            Event(() => StatusPersisted, x => x.CorrelateById(context => context.Message.Id));

            CompositeEvent(() => AllPersisted, x => x.AllPersisted, StatusChanged, StatusPersistenceDone, NodeStatusPersistenceDone);
            CompositeEvent(() => EndProcessing, x => x.EndProcessing, MetadataExtractionFinished, ImageGenerationFinished);

            Initially(
                When(ProcessFile)
                    .TransitionTo(Processing)
                    .ThenAsync(async context =>
                    {
                        Log.Debug($"MicroscopyFile: ProcessMicroscopyFile {context.Data.Id}");

                        context.Instance.Created = DateTimeOffset.UtcNow;
                        context.Instance.Updated = DateTimeOffset.UtcNow;
                        context.Instance.FileId = context.Data.Id;
                        context.Instance.ParentId = context.Data.ParentId;
                        context.Instance.UserId = context.Data.UserId;
                        context.Instance.BlobId = context.Data.BlobId;
                        context.Instance.Bucket = context.Data.Bucket;

                        await context.Raise(BeginProcessing);
                    })
            );

            During(Processing,
                Ignore(NodeStatusPersisted),
                Ignore(StatusPersisted),
                When(BeginProcessing)
                    .ThenAsync(async context =>
                    {
                        await context.CreateConsumeContext().Publish<ChangeStatus>(new
                        {
                            Id = context.Instance.FileId,
                            Status = FileStatus.Processing,
                            UserId = context.Instance.UserId
                        });
                    }),
                When(StatusChanged)
                    .ThenAsync(async context =>
                    {
                        await context.CreateConsumeContext().GenerateImage(context.Instance, 300, 300);
                        await context.CreateConsumeContext().GenerateImage(context.Instance, 600, 600);
                        await context.CreateConsumeContext().GenerateImage(context.Instance, 1200, 1200);

                        await context.CreateConsumeContext().Publish<ExtractMicroscopyMetadata>(new
                        {
                            Id = context.Instance.FileId,
                            UserId = context.Instance.UserId,
                            BlobId = context.Instance.BlobId,
                            Bucket = context.Instance.Bucket,
                            CorrelationId = context.Instance.CorrelationId
                        });
                    }),
                When(ImageGenerated)
                    .ThenAsync(async context => {
                        if (context.Data.TimeStamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.TimeStamp;

                        await context.CreateConsumeContext().Publish<AddImage>(new
                        {
                            Id = context.Instance.FileId,
                            UserId = context.Instance.UserId,
                            Image = new Image(context.Instance.Bucket, context.Data.Image.Id, context.Data.Image.Format, context.Data.Image.MimeType, context.Data.Image.Width, context.Data.Image.Height, context.Data.Image.Exception)
                        });
                    }),
                When(ImageGenerationFailed)
                    .ThenAsync(async context => {
                        if (context.Data.TimeStamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.TimeStamp;

                        context.Instance.Images.Add(context.Data.Image.Id);

                        if (context.Instance.Images.Count == 3)
                        {
                            await context.Raise(ImageGenerationFinished);
                        }
                    }),
                When(ImageAdded)
                    .ThenAsync(async context =>
                    {
                        context.Instance.Images.Add(context.Data.Image.Id);

                        if (context.Instance.Images.Count == 3)
                        {
                            await context.Raise(ImageGenerationFinished);
                        }
                    }),
                When(MetadataExtracted)
                    .ThenAsync(async context => {
                        if (context.Data.TimeStamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.TimeStamp;

                        await context.CreateConsumeContext().Publish<UpdateBioMetadata>(new
                        {
                            Id = context.Instance.FileId,
                            UserId = context.Instance.UserId,
                            Metadata = context.Data.Metadata.Select(i => new KeyValue<string> { Name = i.Key, Value = i.Value.ToString()})
                        });
                    }),
                When(MetadataExtractionFailed)
                    .ThenAsync(async context => {
                        if (context.Data.TimeStamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.TimeStamp;

                        await context.Raise(MetadataExtractionFinished);
                    }),
                When(MetadataPersisted)
                    .ThenAsync(async context =>
                    {
                        await context.Raise(MetadataExtractionFinished);
                    }),
                When(EndProcessing)
                    .TransitionTo(Processed)
                    .ThenAsync(async context =>
                    {
                        await context.Raise(BeginProcessed);
                    })
            );

            During(Processed,
                When(BeginProcessed)
                    .ThenAsync(async context =>
                    {
                        await context.CreateConsumeContext().Publish<ChangeStatus>(new
                        {
                            Id = context.Instance.FileId,
                            UserId = context.Instance.UserId,
                            Status = FileStatus.Processed
                        });
                    }),
                When(NodeStatusPersisted)
                    .ThenAsync(async context =>
                    {
                        if (context.Data.Status != FileStatus.Processing)
                        {
                            await context.Raise(NodeStatusPersistenceDone);
                        }
                    }),
                When(StatusPersisted)
                    .ThenAsync(async context =>
                    {
                        if (context.Data.Status != FileStatus.Processing)
                        {
                            await context.Raise(StatusPersistenceDone);
                        }
                    }),
                When(AllPersisted)
                    .ThenAsync(async context =>
                    {
                        await context.Raise(EndProcessed);
                    }),
                When(EndProcessed)
                    .ThenAsync(async context =>
                    {
                        Log.Debug($"GenericFile: EndProcessed {context.Instance.FileId}");

                        await context.CreateConsumeContext().Publish<FileProcessed>(new
                        {
                            Id = context.Instance.FileId,
                            ParentId = context.Instance.ParentId,
                            BlobId = context.Instance.BlobId,
                            Bucket = context.Instance.Bucket,
                            CorrelationId = context.Instance.CorrelationId,
                            TimeStamp = DateTimeOffset.UtcNow
                        });
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        public Event<ProcessMicroscopyFile> ProcessFile { get; private set; }
        Event<ImageGenerated> ImageGenerated { get; set; }
        Event<ImageGenerationFailed> ImageGenerationFailed { get; set; }
        Event<ImageAdded> ImageAdded { get; set; }
        Event<MicroscopyMetadataExtracted> MetadataExtracted { get; set; }
        Event<MicroscopyMetadataExtractionFailed> MetadataExtractionFailed { get; set; }
        Event<BioMetadataPersisted> MetadataPersisted { get; set; }
        Event<FileProcessed> FileProcessed { get; set; }
        Event<StatusChanged> StatusChanged { get; set; }
        Event<NodeStatusPersisted> NodeStatusPersisted { get; set; }
        Event<StatusPersisted> StatusPersisted { get; set; }

        State Processing { get; set; }
        Event BeginProcessing { get; set; }
        Event EndProcessing { get; set; }
        State Processed { get; set; }
        Event BeginProcessed { get; set; }
        Event EndProcessed { get; set; }

        Event AllPersisted { get; set; }
        Event NodeStatusPersistenceDone { get; set; }
        Event StatusPersistenceDone { get; set; }
        Event ImageGenerationFinished { get; set; }
        Event MetadataExtractionFinished { get; set; }
    }
}
