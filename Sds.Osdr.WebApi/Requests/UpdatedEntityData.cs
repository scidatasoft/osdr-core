using Sds.Osdr.Generic.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Sds.Osdr.WebApi.Requests
{
    public class UpdatedEntityData
    {
        public Guid Id { get; set; }
        public UpdatedEntityData()
        {
            Permissions = new AccessPermissions
            {
                Users = new HashSet<Guid>(),
                Groups = new HashSet<Guid>()
            };
            Metadata = new Dictionary<string, object>();
        }

        public Guid? ParentId { get; set; } = Guid.Empty;
        public string Name { get; set; } = null;
        public AccessPermissions Permissions { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
    }
}
