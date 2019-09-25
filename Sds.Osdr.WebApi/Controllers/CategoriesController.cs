using Leanda.Categories.Domain.Commands;
using Leanda.Categories.Domain.ValueObjects;
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

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [UserInfoRequired]
    public class CategoriesController : MongoDbController
    {
        private IBusControl Bus;
        private IMongoCollection<BsonDocument> CategoryTreeCollection;

        //IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoriesController(IMongoDatabase database, IBusControl bus /*IElasticClient elasticClient*/, IUrlHelper urlHelper) : base(database)
        {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            CategoryTreeCollection = Database.GetCollection<BsonDocument>("CategoryTrees");

            //_elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <summary>
        /// Returns all available categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategoryTree()
        {
            var result = await CategoryTreeCollection.Find(_ => true).Project<dynamic>(@"{
                    CreatedBy:1,
                    CreatedDateTime:1,
                    UpdatedBy:1,
                    UpdatedDateTime:1,
                    Version:1
                }").ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// Create new category tree
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCategoryTree([FromBody] List<TreeNode> nodes)
        {
            Guid categoriesTreeId = Guid.NewGuid();

            await Bus.Publish<CreateCategoryTree>(new
            {
                Id = categoriesTreeId,
                UserId = UserId,
                Nodes = nodes
            });

            return CreatedAtRoute("GetCategoriesTree", new { id = categoriesTreeId }, categoriesTreeId.ToString());
        }

        /// <summary>
        /// Get categories tree by Id
        /// </summary>
        /// <param name="id">Caregories tree aggregate ID</param>
        /// <returns></returns>
        [HttpGet("{id}/tree", Name = "GetCategoriesTree")]
        public async Task<IActionResult> GetCategoriesTree(Guid id)
        {
            var tree = await CategoryTreeCollection.Find(new BsonDocument("_id", id))
                .Project<dynamic>(@"{
                    CreatedBy:1,
                    CreatedDateTime:1,
                    UpdatedBy:1,
                    UpdatedDateTime:1,
                    Version:1,
                    Nodes:1
                }")
                .FirstOrDefaultAsync();

            if (tree == null)
            {
                return BadRequest();
            }

            return Ok(tree);
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
            await Bus.Publish<UpdateCategoryTree>(new
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
            await Bus.Publish<UpdateCategoryTree>(new
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