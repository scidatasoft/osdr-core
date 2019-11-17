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
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Leanda.Categories.Domain;

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    //[UserInfoRequired]
    public class CategoryEntitiesController : MongoDbController, IPaginationController
    {
        private IBusControl _bus;
        private IMongoCollection<BsonDocument> _nodesTreeCollection;
        private readonly ISession _session;

        IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoryEntitiesController(IMongoDatabase database, IBusControl bus, IElasticClient elasticClient, IUrlHelper urlHelper, ISession session) : base(database)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            _nodesTreeCollection = Database.GetCollection<BsonDocument>("Nodes");
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <summary>
        /// Add categories to entity
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="categoriesIds">List of categories ID</param>
        /// <returns></returns>
        [HttpPost("entities/{entityId}/categories")]
        public async Task<IActionResult> AddEntityCategories(Guid entityId, [FromBody] IEnumerable<Guid> categoriesIds)
        {
            var node = await _nodesTreeCollection.Find(new BsonDocument("_id", entityId)).FirstOrDefaultAsync();
            if (node == null)
                return NotFound();

            await _bus.Publish<AddEntityCategories>(new
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                CategoriesIds = categoriesIds,
                UserId
            });

            return Accepted();
        }

        /// <summary>
        /// Delete categories by categoryId
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpDelete("entities/{entityId}/categories/{categoryId}")]
        public async Task<IActionResult> DeleteEntityCategory(Guid entityId, Guid categoryId)
        {
            var node = await _nodesTreeCollection.Find(new BsonDocument("_id", entityId)).FirstOrDefaultAsync();
            if (node == null)
                return NotFound();

            await _bus.Publish<DeleteEntityCategories>(new
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                CategoriesIds = new List<Guid> { categoryId },
                UserId
            });

            return Accepted();
        }

        /// <summary>
        /// Delete categories by categoriesIds
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="categoriesIds">List of categories ID</param>
        /// <returns></returns>
        [HttpDelete("entities/{entityId}/categories")]
        public async Task<IActionResult> DeleteEntityCategories(Guid entityId, [FromBody][Required] IEnumerable<Guid> categoriesIds)
        {
            var node = await _nodesTreeCollection.Find(new BsonDocument("_id", entityId)).FirstOrDefaultAsync();
            if (node == null)
                return NotFound();

            await _bus.Publish<DeleteEntityCategories>(new
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                CategoriesIds = categoriesIds,
                UserId
            });

            return Accepted();
        }

        /// <summary>
        /// Get entities by categoryId
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="paginationRequest">Pagination request (pageSize, pageNumber)</param>
        /// <returns></returns>
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetEntitiesByCategoryId(Guid categoryId, PaginationRequest paginationRequest)
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

        /// <summary>
        /// Get categories ids by entityId
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns></returns>
        [HttpGet("entities/{entityId}/categories")]
        [ProducesResponseType(typeof(IEnumerable<Guid>), 200)]
        public async Task<IActionResult> GetCategoriesIdsByEntityId(Guid entityId)
        {
            var node = await _nodesTreeCollection.Find(new BsonDocument("_id", entityId)).FirstOrDefaultAsync();
            if (node == null)
                return NotFound();

            var hits = _elasticClient.Search<dynamic>(s => s
                .Index("categories")
                .Type("category")
                .Query(q => q.QueryString(qs => qs.Query(entityId.ToString()))))
                .Hits.ToArray();

            IEnumerable<Guid> categoriesIds = new List<Guid>();

            if (hits.Any())
            {
                JObject hitObject = JsonConvert.DeserializeObject<JObject>(hits[0].Source.ToString());
                categoriesIds = hitObject.Value<JArray>("CategoriesIds").Select(x => Guid.Parse(x.ToString()));
            }
            return Ok(categoriesIds);
        }

        [NonAction]
        public string CreatePageUri(PaginationRequest request, PaginationUriType uriType, string action, Guid? containerId = null, string filter = null, IEnumerable<string> fields = null)
        {
            int pageNumber = uriType == PaginationUriType.PreviousPage ? request.PageNumber - 1 : request.PageNumber + 1;

            return _urlHelper.Link(action, new { query = filter, pageSize = request.PageSize, pageNumber });

        }

        [NonAction]
        public string CreatePageUri(PaginationRequest request, PaginationUriType uriType, string action, RouteValueDictionary routeValueDictionary)
        {
            int pageNumber = uriType == PaginationUriType.PreviousPage ? request.PageNumber - 1 : request.PageNumber + 1;

            return _urlHelper.Link(action, routeValueDictionary);
        }
    }
}
