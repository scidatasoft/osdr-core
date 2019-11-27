using Sds.Osdr.Domain;
using Sds.Osdr.Generic.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Sds.Osdr.WebApi.Requests
{
    public class UpdateEntityRequest
    {
        public Guid Id { get; set; }
        public UpdateEntityRequest()
        {
            Permissions = new AccessPermissions
            {
                Users = new HashSet<Guid>(),
                Groups = new HashSet<Guid>()
            };
            Metadata = new List<KeyValue<string>>();
        }

        public Guid? ParentId { get; set; } = Guid.Empty;
        public string Name { get; set; } = null;
        public AccessPermissions Permissions { get; set; }
        public IEnumerable<KeyValue<string>> Metadata { get; set; }
    }
}
