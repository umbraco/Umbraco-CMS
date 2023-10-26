using Asp.Versioning;
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
public class AllowedChildrenDocumentController : DocumentControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentCreatingService _contentCreatingService;

    public AllowedChildrenDocumentController(IUmbracoMapper umbracoMapper, IContentCreatingService contentCreatingService)
    {
        _umbracoMapper = umbracoMapper;
        _contentCreatingService = contentCreatingService;
    }

    [HttpGet("allowed-document-types")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedChildrenByKey(Guid? parentId, int skip = 0, int take = 100)
    {
        PagedModel<IContentType> allowedChildren;

        if (parentId.HasValue is false)
        {
            allowedChildren = await _contentCreatingService.GetAllowedChildrenContentTypesAtRootAsync(skip, take);
        }
        else
        {
            Attempt<PagedModel<IContentType>?, ContentCreatingOperationStatus> allowedChildrenAttempt =
                await _contentCreatingService.GetAllowedChildrenContentTypesAsync(parentId.Value, skip, take);

            if (allowedChildrenAttempt.Success is false)
            {
                return ContentCreatingOperationStatusResult(allowedChildrenAttempt.Status);
            }

            allowedChildren = allowedChildrenAttempt.Result!;
        }

        List<DocumentTypeResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeResponseModel>(allowedChildren.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeResponseModel>
        {
            Total = allowedChildren.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
