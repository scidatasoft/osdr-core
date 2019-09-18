using CQRSlite.Domain.Exception;
using Leanda.Categories.Domain.Events;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Leanda.Categories.Domain.Persistence.EventHandlers
{
    public class FolderEventHandlers : IConsumer<CategoryTreeCreated>,
		                               IConsumer<CategoryTreeUpdated>
    {
		private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _categoryTreeCollection;

        public FolderEventHandlers(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _categoryTreeCollection = _database.GetCollection<BsonDocument>("CategoryTrees");
        }

		public async Task Consume(ConsumeContext<CategoryTreeCreated> context)
		{
			var tree = new
			{
				CreatedBy = context.Message.UserId,
				CreatedDateTime = context.Message.TimeStamp.UtcDateTime,
                UpdatedBy = context.Message.UserId,
                UpdatedDateTime = context.Message.TimeStamp.UtcDateTime,
                Id = context.Message.Id,
				Version = context.Message.Version,
                Nodes = context.Message.Nodes
			}.ToBsonDocument();

			await _categoryTreeCollection.InsertOneAsync(tree);

            await context.Publish<CategoryTreePersisted>(new
            {
                Id = context.Message.Id,
                TimeStamp = DateTimeOffset.UtcNow,
                Version = context.Message.Version
            });
        }

        public async Task Consume(ConsumeContext<CategoryTreeUpdated> context)
		{
            var filter = new BsonDocument("_id", context.Message.Id).Add("Version", context.Message.Version - 1);

            var update = Builders<BsonDocument>.Update
				.Set("Nodes", context.Message.Nodes)
				.Set("UpdatedBy", context.Message.UserId)
				.Set("UpdatedDateTime", context.Message.TimeStamp.UtcDateTime)
				.Set("Version", context.Message.Version);

            var element = await _categoryTreeCollection.FindOneAndUpdateAsync(filter, update);

            if (element == null)
                throw new ConcurrencyException(context.Message.Id);

            await context.Publish<CategoryTreePersisted>(new
            {
                Id = context.Message.Id,
                TimeStamp = DateTimeOffset.UtcNow,
                Version = context.Message.Version
            });
        }
    }
}
