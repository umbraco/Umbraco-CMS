﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaOrMediaTypes)]
public class AllowedChildrenMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllowedChildrenMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/allowed-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedMediaType>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenByKey(Guid id, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<IMediaType>?, ContentTypeOperationStatus> attempt = await _mediaTypeService.GetAllowedChildrenAsync(id, skip, take);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        List<AllowedMediaType> viewModels = _umbracoMapper.MapEnumerable<IMediaType, AllowedMediaType>(attempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<AllowedMediaType>
        {
            Total = attempt.Result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
