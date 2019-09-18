using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using Leanda.Categories.Domain.Commands;
using Leanda.Categories.Domain.Events;
using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Leanda.Categories.Domain.FrontEnd.CommandHandlers
{
    public class CategoryTreeCommandHandler : IConsumer<CreateCategoryTree>,
                                              IConsumer<UpdateCategoryTree> 
    {
        private readonly ISession _session;

        public CategoryTreeCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<CreateCategoryTree> context)
        {
            var tree = new CategoryTree(context.Message.Id, userId: context.Message.UserId, nodes: context.Message.Nodes);

            await _session.Add(tree);

            await _session.Commit();
        }

        public async Task Consume(ConsumeContext<UpdateCategoryTree> context)
        {
            try
            {
                var tree = await _session.Get<CategoryTree>(context.Message.Id);

                tree.Update(context.Message.UserId, context.Message.ParentId, context.Message.Nodes);

                await _session.Commit();
            }
            catch (ConcurrencyException)
            {
                Log.Error($"Error update category tree {context.Message.Id} unexpected version {context.Message.ExpectedVersion}");

                await context.Publish<UnexpectedVersion>(new
                {
                    Id = context.Message.Id,
                    UserId = context.Message.UserId,
                    Version = context.Message.ExpectedVersion,
                    TimeStamp = DateTimeOffset.UtcNow
                });
            }
        }
    }
}
