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

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Provides API endpoints for managing document blueprints associated with specific document types in Umbraco.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class DocumentBlueprintForDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;

    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBlueprintForDocumentTypeController"/> class, which manages document blueprints for a specific document type.
    /// </summary>
    /// <param name="contentBlueprintEditingService">Service used to edit content blueprints.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    public DocumentBlueprintForDocumentTypeController(IContentBlueprintEditingService contentBlueprintEditingService, IUmbracoMapper umbracoMapper)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged collection of document blueprints for the specified document type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document type.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged collection of document blueprints, or a 404 response if the document type is not found.</returns>
    [HttpGet("{id:guid}/blueprint")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeBlueprintItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets document blueprints for a document type.")]
    [EndpointDescription("Gets a collection of document blueprints available for the specified document type.")]
    public async Task<IActionResult> DocumentBlueprintByDocumentTypeKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IContent>?, ContentEditingOperationStatus> attempt = await _contentBlueprintEditingService.GetPagedByContentTypeAsync(id, skip, take);
        if (attempt.Success is false)
        {
            return ContentEditingOperationStatusResult(attempt.Status);
        }

        List<DocumentTypeBlueprintItemResponseModel> viewModels = _umbracoMapper.MapEnumerable<IContent, DocumentTypeBlueprintItemResponseModel>(attempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<DocumentTypeBlueprintItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
