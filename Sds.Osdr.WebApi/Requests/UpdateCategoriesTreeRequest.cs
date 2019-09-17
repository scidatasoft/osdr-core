using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sds.Osdr.WebApi.Requests
{
    public class UpdateCategoriesTreeRequest
    {
        public Guid? Id { get; set; }
        public UpdateCategoriesTreeRequest[] Children{ get; set; }
    }
}
