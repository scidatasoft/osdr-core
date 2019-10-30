using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using Leanda.Categories.Domain;
using Leanda.Categories.Domain.Commands;
using Leanda.Categories.Domain.Events;
using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Leanda.Categories.BackEnd.CommandHandlers
{
    public class EntityCategoriesCommandHandler : IConsumer<AddEntityCategories>
    {
        private readonly ISession _session;

        public EntityCategoriesCommandHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<AddEntityCategories> context)
        {
            var entityCategory = new EntityCategories(context.Message.Id, userId: context.Message.UserId, categoriesIds: context.Message.CategoriesIds);

            await _session.Add(entityCategory);

            await _session.Commit();
        }
    }
}
