using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Sds.Osdr.WebApi.Extensions;
using Sds.Osdr.WebApi.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using Sds.Osdr.WebApi.Responses;
using Serilog;
using Sds.Osdr.WebApi.Filters;
using Microsoft.AspNetCore.Routing;
using System.Dynamic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Leanda.Categories.Domain.ValueObjects;
using System.Threading.Tasks;
using MassTransit;

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [UserInfoRequired]
    public class CategoriesController : ControllerBase
    {
        private IBusControl _bus;

        //IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoriesController(IBusControl bus /*IElasticClient elasticClient, IUrlHelper urlHelper*/)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            //_urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            //_elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <summary>
        /// Returns all available categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public IActionResult GetAllCategoryTree()
        {
            //  TODO: Connect to MongoDB and return all available categories here. Pagination required.

            return Ok();
        }

        /// <summary>
        /// Create new category tree
        /// </summary>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<IActionResult> CreateCategoryTree([FromBody] List<TreeNode> nodes)
        {

            await _bus.Publish<CreateFolder>(new
            {
                Id = folderId,
                UserId = UserId,
                Name = request.Name,
                ParentId = request.ParentId
            });

            return AcceptedAtRoute("GetSingleEntity", new { type = "folders", id = folderId }, null);
        }

        /// <summary>
        /// Get categories tree by Id
        /// </summary>
        /// <param name="id">Caregories tree aggregate ID</param>
        /// <returns></returns>
        [HttpGet("{id}/tree")]
        public IActionResult GetTree(Guid id)
        {
            //  TODO: Connect to MongoDB and return requested Categories Tree by ID

            return Ok();
        }

        /// <summary>
        /// Update categories tree
        /// </summary>
        /// <param name="id">Categories tree ID</param>
        /// <param name="nodes">New categories tree nodes</param>
        /// <returns></returns>
        [HttpPut("{id}/tree")]
        public IActionResult UpdateCategoriesTree(Guid id, [FromBody] List<TreeNode> nodes)
        {

            return Ok();
        }

        /// <summary>
        /// Update categories tree node
        /// </summary>
        /// <param name="id">Categories tree ID</param>
        /// <param name="nodeId">Categories tree node ID</param>
        /// <param name="nodes">New categories tree nodes</param>
        /// <returns></returns>
        [HttpPut("{id}/tree/{nodeId}")]
        public IActionResult UpdateCategoriesTreeNode(Guid id, Guid nodeId, [FromBody] List<TreeNode> nodes)
        {

            return Ok();
        }
    }
}