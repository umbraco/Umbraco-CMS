using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

    /// <summary>
    /// Controller responsible for managing the allowed child media types for a given media type.
    /// </summary>
[ApiVersion("1.0")]
public class AllowedChildrenMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedChildrenMediaTypeController"/> class, which manages allowed child media types for a given media type.
    /// </summary>
    /// <param name="mediaTypeService">Service used to manage media types.</param>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco domain models and API models.</param>
    public AllowedChildrenMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged collection of media types that are allowed as children of the specified media type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (key) of the parent media type.</param>
    /// <param name="parentContentKey">An optional key of the parent content item to further filter allowed children.</param>
    /// <param name="skip">The number of items to skip when paging results.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedMediaType}"/> of allowed child media types, or a 404 if not found.</returns>
    [HttpGet("{id:guid}/allowed-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedMediaType>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets allowed child media types.")]
    [EndpointDescription("Gets a collection of media types that are allowed as children of the specified parent media type.")]
    public async Task<IActionResult> AllowedChildrenByKey(
        CancellationToken cancellationToken,
        Guid id,
        Guid? parentContentKey = null,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<IMediaType>?, ContentTypeOperationStatus> attempt = await _mediaTypeService.GetAllowedChildrenAsync(id, parentContentKey, skip, take);
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
