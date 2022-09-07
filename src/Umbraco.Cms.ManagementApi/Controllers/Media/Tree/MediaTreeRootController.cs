﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.Tree;

public class MediaTreeRootController : MediaTreeControllerBase
{
    public MediaTreeRootController(
        IEntityService entityService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<TreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Root(long pageNumber = 0, int pageSize = 100)
        => await GetRoot(pageNumber, pageSize);
}
