using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

/// <summary>
/// Provides API endpoints for searching media type items in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class SearchMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchMediaTypeItemController"/> class.
    /// </summary>
    /// <param name="entitySearchService">Service used to search for entities.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    /// <param name="mapper">The Umbraco object mapper.</param>
    public SearchMediaTypeItemController(IEntitySearchService entitySearchService, IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Searches for media type items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter media type items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paged collection of <see cref="MediaTypeItemResponseModel"/> objects.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches media type items.")]
    [EndpointDescription("Searches media type items by the provided query with pagination support.")]
    public Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MediaType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Task.FromResult<IActionResult>(Ok(new PagedModel<MediaTypeItemResponseModel> { Total = searchResult.Total }));
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray().EmptyNull());
        var result = new PagedModel<MediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes),
            Total = searchResult.Total
        };

        return Task.FromResult<IActionResult>(Ok(result));
    }
}
