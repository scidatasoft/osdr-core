using Leanda.Categories.Domain.Commands;
using Leanda.Categories.Domain.ValueObjects;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using Sds.Osdr.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [UserInfoRequired]
    public class CategoriesController : MongoDbController
    {
        private IBusControl _bus;

        //IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoriesController(IMongoDatabase database, IBusControl bus /*IElasticClient elasticClient*/, IUrlHelper urlHelper) : base(database)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));

            //_elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <summary>
        /// Returns all available categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategoryTree()
        {
            //  TODO: Connect to MongoDB and return all available categories here. Pagination required.

            return Ok();
        }

        /// <summary>
        /// Create new category tree
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCategoryTree([FromBody] List<TreeNode> nodes)
        {
            Guid categoriesTreeId = Guid.NewGuid();

            await _bus.Publish<CreateCategoryTree>(new
            {
                Id = categoriesTreeId,
                UserId = UserId,
                Nodes = nodes
            });

            return AcceptedAtRoute("GetCategoriesTree", new { id = categoriesTreeId }, null);
        }

        /// <summary>
        /// Get categories tree by Id
        /// </summary>
        /// <param name="id">Caregories tree aggregate ID</param>
        /// <returns></returns>
        [HttpGet("{id}/tree", Name = "GetCategoriesTree")]
        public async Task<IActionResult> GetCategoriesTree(Guid id)
        {
            //  TODO: Connect to MongoDB and return requested Categories Tree by ID

            return Ok();
        }

        /// <summary>
        /// Update categories tree
        /// </summary>
        /// <param name="id">Categories tree ID</param>
        /// <param name="nodes">New categories tree nodes</param>
        /// <param name="version">Carrent cattegories tree object version</param>
        /// <returns></returns>
        [HttpPut("{id}/tree")]
        public async Task<IActionResult> UpdateCategoriesTree(Guid id, [FromBody] List<TreeNode> nodes, int version)
        {
            await _bus.Publish<UpdateCategoryTree>(new
            {
                Id = id,
                UserId = UserId,
                Nodes = nodes,
                ExpectedVersion = version
            });

            return Accepted();
        }

        /// <summary>
        /// Update categories tree node
        /// </summary>
        /// <param name="id">Categories tree ID</param>
        /// <param name="nodeId">Categories tree node ID</param>
        /// <param name="nodes">New categories tree nodes</param>
        /// <param name="version">Carrent cattegories tree object version</param>
        /// <returns></returns>
        [HttpPut("{id}/tree/{nodeId}")]
        public async Task<IActionResult> UpdateCategoriesTreeNode(Guid id, Guid nodeId, [FromBody] List<TreeNode> nodes, int version)
        {
            await _bus.Publish<UpdateCategoryTree>(new
            {
                Id = id,
                ParentId = nodeId,
                UserId = UserId,
                Nodes = nodes,
                ExpectedVersion = version
            });

            return Accepted();
        }
    }
}