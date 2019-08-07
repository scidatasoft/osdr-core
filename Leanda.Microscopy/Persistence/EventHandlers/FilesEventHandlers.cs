using CQRSlite.Domain.Exception;
using Leanda.Microscopy.Domain.Events;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leanda.Microscopy.Persistence.EventHandlers
{
    public class FilesEventHandlers : IConsumer<MicroscopyFileCreated>,
                                      IConsumer<BioMetadataUpdated>
    {
        private readonly IMongoDatabase database;

        private IMongoCollection<BsonDocument> Files { get { return database.GetCollection<BsonDocument>("Files"); } }

        public FilesEventHandlers(IMongoDatabase database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task Consume(ConsumeContext<MicroscopyFileCreated> context)
        {
            var filter = new BsonDocument("_id", context.Message.Id).Add("Version", context.Message.Version - 1);
            var update = Builders<BsonDocument>.Update
                .Set("UpdatedBy", context.Message.UserId)
                .Set("UpdatedDateTime", context.Message.TimeStamp.UtcDateTime)
                .Set("Version", context.Message.Version);

            var document = await Files.FindOneAndUpdateAsync(filter, update);

            if (document == null)
            {
                throw new ConcurrencyException(context.Message.Id);
            }
        }

        public async Task Consume(ConsumeContext<BioMetadataUpdated> context)
        {
            var filter = new BsonDocument("_id", context.Message.Id).Add("Version", context.Message.Version - 1);
            var update = Builders<BsonDocument>.Update
                //.Set("Properties", (new { BioMetadata = context.Message.Metadata.Select(m => new { Name = m.Key, Value = m.Value }) }).ToBsonDocument())
                .Set("Properties", (new { BioMetadata = context.Message.Metadata.Select(m => new KeyValue<string> { Name = m.Key, Value = m.Value.ToString() }) }).ToBsonDocument())
                .Set("UpdatedBy", context.Message.UserId)
                .Set("UpdatedDateTime", context.Message.TimeStamp.UtcDateTime)
                .Set("Version", context.Message.Version);

            var document = await Files.FindOneAndUpdateAsync(filter, update);

            if (document == null)
            {
                throw new ConcurrencyException(context.Message.Id);
            }

            await context.Publish<BioMetadataPersisted>(new
            {
                Id = context.Message.Id,
                UserId = context.Message.UserId,
                TimeStamp = DateTimeOffset.UtcNow
            });
        }
    }
}
