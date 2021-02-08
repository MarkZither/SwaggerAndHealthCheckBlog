using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResourceService.Controllers
{
    //[ApiController]
    [Route("[controller]")]
    [Route("/{__tenant__=}/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ILogger<TenantController> _logger;

        // The Web API will only accept tokens 1) for users, and 2) having the access_as_user scope for this API
        private static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        public TenantController(ILogger<TenantController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;

            if (tenantInfo != null)
            {
                var tenantId = tenantInfo.Id;
                var identifier = tenantInfo.Identifier;
                var name = tenantInfo.Name;

                return Ok(tenantInfo);
            }

            return NotFound();
        }
    }
}
