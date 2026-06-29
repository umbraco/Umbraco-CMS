using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// API controller responsible for managing media types that are allowed to be created at the root level of the media section.
/// </summary>
[ApiVersion("1.0")]
public class AllowedAtRootMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedAtRootMediaTypeController"/> class, which manages allowed media types at the root level.
    /// </summary>
    /// <param name="mediaTypeService">The service used to manage media types.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public AllowedAtRootMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged list of media types that can be created at the root level.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{AllowedMediaType}"/> representing the allowed media types.</returns>
    [HttpGet("allowed-at-root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AllowedMediaType>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets media types allowed at root.")]
    [EndpointDescription("Gets a collection of media types that are allowed to be created at the root level.")]
    public async Task<IActionResult> AllowedAtRoot(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IMediaType> result = await _mediaTypeService.GetAllAllowedAsRootAsync(skip, take);

        List<AllowedMediaType> viewModels = _umbracoMapper.MapEnumerable<IMediaType, AllowedMediaType>(result.Items);

        var pagedViewModel = new PagedViewModel<AllowedMediaType>
        {
            Total = result.Total,
            Items = viewModels,
        };

        return Ok(pagedViewModel);
    }
}
