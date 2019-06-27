using CQRSlite.Domain;
using Leanda.Microscopy.Domain;
using Leanda.Microscopy.Domain.Commands;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Leanda.Microscopy.BackEnd.CommandHandlers
{
    public class UpdateMetadataCommandHandler : IConsumer<UpdateMetadata>
    {
        private readonly ISession session;

        public UpdateMetadataCommandHandler(ISession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<UpdateMetadata> context)
        {
            var file = await session.Get<MicroscopyFile>(context.Message.Id);

            file.UpdateMetadata(context.Message.UserId, context.Message.Metadata);

            await session.Commit();
        }
    }
}
