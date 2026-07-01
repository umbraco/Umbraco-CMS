using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Controller responsible for managing the allowed child document types for a given document type.
/// </summary>
[ApiVersion("1.0")]
public class AllowedChildrenDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedChildrenDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">Service used to manage content types within the controller.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public AllowedChildrenDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged list of document types that can be created as children of the specified parent document type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (key) of the parent document type.</param>
    /// <param name="parentContentKey">An optional key of the parent content item to further filter allowed children.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedDocumentType}"/> of allowed child document types.</returns>
    [HttpGet("{id:guid}/allowed-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedDocumentType>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets allowed child document types.")]
    [EndpointDescription("Gets a collection of document types that are allowed as children of the specified parent document type.")]
    public async Task<IActionResult> AllowedChildrenByKey(
        CancellationToken cancellationToken,
        Guid id,
        Guid? parentContentKey = null,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IContentType>?, ContentTypeOperationStatus> attempt = await _contentTypeService.GetAllowedChildrenAsync(id, parentContentKey, skip, take);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        List<AllowedDocumentType> viewModels = _umbracoMapper.MapEnumerable<IContentType, AllowedDocumentType>(attempt.Result!.Items);

        var pagedViewModel = new PagedViewModel<AllowedDocumentType>
        {
            Total = attempt.Result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
