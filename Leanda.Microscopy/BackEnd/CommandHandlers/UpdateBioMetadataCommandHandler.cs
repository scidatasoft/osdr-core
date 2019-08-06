using CQRSlite.Domain;
using Leanda.Microscopy.Domain;
using Leanda.Microscopy.Domain.Commands;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Leanda.Microscopy.BackEnd.CommandHandlers
{
    public class UpdateBioMetadataCommandHandler : IConsumer<UpdateBioMetadata>
    {
        private readonly ISession session;

        public UpdateBioMetadataCommandHandler(ISession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<UpdateBioMetadata> context)
        {
            var file = await session.Get<MicroscopyFile>(context.Message.Id);

            file.UpdateMetadata(context.Message.UserId, context.Message.Metadata);

            await session.Commit();
        }
    }
}
