using CQRSlite.Domain;
using MassTransit;
using Sds.Osdr.Generic.Domain.Events;
using Sds.Osdr.Generic.Domain.Commands.Files;
using Serilog;
using System;
using System.Threading.Tasks;
using Sds.Osdr.Generic.Domain;

namespace Sds.Osdr.Generic.BackEnd.CommandHandlers.Files
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
            var file = await session.Get<File>(context.Message.Id);
            if (file.Version == context.Message.ExpectedVersion)
            {
                file.UpdateMetadata(context.Message.UserId, context.Message.Metadata);
                await session.Commit();
            }
            else
            {
                Log.Error($"Unexpected version for record '{context.Message.Id}', expected version {context.Message.ExpectedVersion}, found {file.Version}");
                await context.Publish<UnexpectedVersion>(new
                {
                    Id = context.Message.Id,
                    UserId = context.Message.UserId,
                    Version = context.Message.ExpectedVersion,
                    TimeStamp = DateTimeOffset.Now
                });
            }
        }
    }
}
