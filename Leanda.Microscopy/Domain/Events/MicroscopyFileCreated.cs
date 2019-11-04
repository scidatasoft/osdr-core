using Sds.CqrsLite.Events;
using System;

namespace Leanda.Microscopy.Domain.Events
{
    public class MicroscopyFileCreated : IUserEvent
    {
		public MicroscopyFileCreated(Guid id)
		{
			Id = id;
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

        public int Version { get; set; }
	}
}
