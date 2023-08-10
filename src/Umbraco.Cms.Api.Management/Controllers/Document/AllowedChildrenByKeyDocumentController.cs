﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public class AllowedChildrenByKeyDocumentController : DocumentControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentCreatingService _contentCreatingService;

    public AllowedChildrenByKeyDocumentController(IUmbracoMapper umbracoMapper, IContentCreatingService contentCreatingService)
    {
        _umbracoMapper = umbracoMapper;
        _contentCreatingService = contentCreatingService;
    }

    [HttpGet("{id:guid}/allowed-document-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenByKey(Guid id, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<IContentType>?, ContentCreatingOperationStatus> allowedChildrenAttempt = await _contentCreatingService.GetAllowedChildrenContentTypesAsync(id, skip, take);

        if (allowedChildrenAttempt.Success is false)
        {
            return ContentCreatingOperationStatusResult(allowedChildrenAttempt.Status);
        }

        List<DocumentTypeResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeResponseModel>(allowedChildrenAttempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeResponseModel>
        {
            Total = allowedChildrenAttempt.Result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
