using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Controller responsible for managing the document types allowed in the library.
/// </summary>
[ApiVersion("1.0")]
public class AllowedInLibraryDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedInLibraryDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">Service used to manage content types within the controller.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public AllowedInLibraryDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged list of document types that are allowed in the library.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedDocumentType}"/> of document types allowed in the library.</returns>
    [NonAction]
    [Obsolete("Please use the overload that accepts a parentKey parameter. Scheduled for removal in Umbraco 20.")]
    public async Task<IActionResult> AllowedInLibrary(CancellationToken cancellationToken, int skip = 0, int take = 100)
        => await AllowedInLibrary(cancellationToken, null, skip, take);

    /// <summary>
    /// Retrieves a paged list of document types that are allowed in the library.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentKey">An optional key of the parent container to further filter the document types allowed in the library, or <c>null</c> at the library root.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedDocumentType}"/> of document types allowed in the library.</returns>
    [HttpGet("allowed-in-library")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedDocumentType>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets document types allowed in library.")]
    [EndpointDescription("Gets a collection of document types that are allowed in the library.")]
    public async Task<IActionResult> AllowedInLibrary(
        CancellationToken cancellationToken,
        Guid? parentKey = null,
        int skip = 0,
        int take = 100)
    {
        PagedModel<IContentType> result = await _contentTypeService.GetAllAllowedInLibraryAsync(parentKey, skip, take);

        List<AllowedDocumentType> viewModels = _umbracoMapper.MapEnumerable<IContentType, AllowedDocumentType>(result.Items);

        var pagedViewModel = new PagedViewModel<AllowedDocumentType>
        {
            Total = result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
