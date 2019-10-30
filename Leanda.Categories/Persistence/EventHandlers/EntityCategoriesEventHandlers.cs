using CQRSlite.Domain.Exception;
using Leanda.Categories.Domain.Events;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Leanda.Categories.Persistence.EventHandlers
{
    public class EntityCategoriesEventHandlers : IConsumer<EntityCategoriesCreated>
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _categoryTreeCollection;

        public EntityCategoriesEventHandlers(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _categoryTreeCollection = _database.GetCollection<BsonDocument>("EntityCategories");
        }

        public async Task Consume(ConsumeContext<EntityCategoriesCreated> context)
        {
            var entityCategories = new
            {
                CreatedBy = context.Message.UserId,
                CreatedDateTime = context.Message.TimeStamp.UtcDateTime,
                UpdatedBy = context.Message.UserId,
                UpdatedDateTime = context.Message.TimeStamp.UtcDateTime,
                Id = context.Message.Id,
                Version = context.Message.Version,
                CategoriesIds = context.Message.CategoriesIds
            }.ToBsonDocument();

            await _categoryTreeCollection.InsertOneAsync(entityCategories);

            await context.Publish<IEntityCategoriesAddPersisted>(new
            {
                Id = context.Message.Id,
                TimeStamp = DateTimeOffset.UtcNow,
                Version = context.Message.Version
            });
        }
    }
}
