using Leanda.Categories.Domain.Commands;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Osdr.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CQRSlite.Domain;
using Nest;
using Sds.Osdr.WebApi.Requests;
using Sds.Osdr.WebApi.Responses;
using System.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using Sds.Osdr.WebApi.Extensions;

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [UserInfoRequired]
    public class CategoryEntitiesController : MongoDbController, IPaginationController
    {
        private IBusControl Bus;
        private IMongoCollection<BsonDocument> CategoryTreeCollection;
        private readonly ISession _session;

        IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoryEntitiesController(IMongoDatabase database, IBusControl bus, IElasticClient elasticClient, IUrlHelper urlHelper, ISession session) : base(database)
        {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            CategoryTreeCollection = Database.GetCollection<BsonDocument>("CategoryTrees");
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }


        [HttpPost("entities/{nodeId}")]
        public async Task<IActionResult> AddCategoriesToEntity(Guid nodeId, [FromBody] List<Guid> categoriesIds)
        {
            // TODO: check entityId exists

            await Bus.Publish<AddCategoriesToEntity>(new
            {
                Id = nodeId,
                CategoriesIds = categoriesIds,
                UserId
            });

            return Accepted();
        }

        [HttpGet("entities/{categoryId}")]
        public IActionResult GetNodes(Guid categoryId, PaginationRequest paginationRequest)
        {
            var result = _elasticClient.Search<dynamic>(s => s
                .Index("categories")
                .Type("category")
                .From((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .Query(q => q.QueryString(qs => qs.Query(categoryId.ToString()))));

            var list = new PagedList<dynamic>(result.Hits.Select(h => JsonConvert.DeserializeObject<ExpandoObject>(h.Source.Node.ToString())), (int)result.Total, paginationRequest.PageNumber, paginationRequest.PageSize);

            this.AddPaginationHeader(paginationRequest, list, "entities", null, categoryId.ToString());

            return Ok(list);
        }

        public string CreatePageUri(PaginationRequest request, PaginationUriType uriType, string action, Guid? containerId = null, string filter = null, IEnumerable<string> fields = null)
        {
            int pageNumber = uriType == PaginationUriType.PreviousPage ? request.PageNumber - 1 : request.PageNumber + 1;

            return _urlHelper.Link(action, new { query = filter, pageSize = request.PageSize, pageNumber });

        }

        public string CreatePageUri(PaginationRequest request, PaginationUriType uriType, string action, RouteValueDictionary routeValueDictionary)
        {
            int pageNumber = uriType == PaginationUriType.PreviousPage ? request.PageNumber - 1 : request.PageNumber + 1;

            return _urlHelper.Link(action, routeValueDictionary);
        }
    }
}
