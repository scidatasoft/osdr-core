using Leanda.Categories.Domain.Commands;
using MassTransit;
using Nest;
using Sds.ChemicalFileParser.Domain;
using Sds.ChemicalFileParser.Domain.Commands;
using Sds.Domain;
using Sds.Storage.Blob.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public class CategoryEntities : IConsumer<AddEntityCategories>,
                                     IConsumer<DeleteEntityCategories>
    {
        private readonly IElasticClient _elasticClient;

        public CategoryEntities(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task Consume(ConsumeContext<AddEntityCategories> context)
        {
            var node = new { _id = context.Message.EntityId, Name = "Moq Name of Node" };
            var insertDocument = new { CategoriesIds = context.Message.CategoriesIds.Distinct(), Node = node };
            var status = await _elasticClient.IndexAsync<dynamic>(insertDocument,
                i => i.Index("categories").Type("category"));
        }

        public async Task Consume(ConsumeContext<DeleteEntityCategories> context)
        {
            await Task.CompletedTask;
        }
    }
}
