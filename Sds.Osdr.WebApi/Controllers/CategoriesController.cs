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

namespace Sds.Osdr.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [UserInfoRequired]
    public class CategoriesController : ControllerBase
    {
        //IElasticClient _elasticClient;
        private IUrlHelper _urlHelper;
        public CategoriesController(/*IElasticClient elasticClient, IUrlHelper urlHelper*/)
        {
            //_urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            //_elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        [HttpGet("tree")]
        public IActionResult GetTree()
        {
            return Ok("Hi");
        }

        [HttpPost("tree")]
        public IActionResult PostTree()
        {

            return Ok("Hi");
        }
    }
}