using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

    /// <summary>
    /// Provides API endpoints for searching and retrieving media items.
    /// </summary>
[ApiVersion("1.0")]
public class SearchMediaItemController : MediaItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchMediaItemController"/> class, which handles search operations for media items.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service used to perform indexed searches on entities.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    /// <param name="dataTypeService">Service for accessing data type information.</param>
    [ActivatorUtilitiesConstructor]
    public SearchMediaItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMediaPresentationFactory mediaPresentationFactory,
        IDataTypeService dataTypeService)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _dataTypeService = dataTypeService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchMediaItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">
    /// The service used to perform indexed searches on entities.
    /// </param>
    /// <param name="mediaPresentationFactory">
    /// The factory responsible for creating media presentation models.
    /// </param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 18.")]
    public SearchMediaItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(
            indexedEntitySearchService,
            mediaPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeService>())
    {
    }

    /// <summary>
    /// Searches for media items under a specified parent, filtered by allowed media types.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query string used to filter media items.</param>
    /// <param name="trashed">Optional. Whether to include trashed media items in the results.</param>
    /// <param name="culture">Optional. The culture to filter the media items by.</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The number of items to return for paging.</param>
    /// <param name="parentId">Optional. The ID of the parent media item to search under.</param>
    /// <param name="allowedMediaTypes">Optional. A list of allowed media type IDs to filter the results.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the search results.</returns>
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SearchFromParentWithAllowedTypes(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedMediaTypes = null)
        => await SearchFromParentWithAllowedTypes(
            cancellationToken,
            query,
            trashed,
            culture,
            skip,
            take,
            parentId,
            allowedMediaTypes,
            null);

    /// <summary>
    /// Searches for media items under a specified parent node, filtered by allowed media types.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="trashed">If set, specifies whether to include trashed items in the search.</param>
    /// <param name="culture">The culture to filter the search by, if any.</param>
    /// <param name="skip">The number of items to skip (for paging).</param>
    /// <param name="take">The maximum number of items to return (for paging).</param>
    /// <param name="parentId">The ID of the parent node to search under, if specified.</param>
    /// <param name="allowedMediaTypes">A list of allowed media type IDs to filter the results, if specified.</param>
    /// <returns>An <see cref="IActionResult"/> containing the filtered search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches media items.")]
    [EndpointDescription("Searches media items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchFromParentWithAllowedTypes(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedMediaTypes = null,
        Guid? dataTypeId = null)
    {
        // We always want to include folders in the search results (aligns with behaviour in Umbraco 13, and allows folders
        // to be selected to find the selectable items inside).
        if (allowedMediaTypes is not null && allowedMediaTypes.Contains(Constants.MediaTypes.Guids.FolderGuid) is false)
        {
            allowedMediaTypes = [.. allowedMediaTypes, Constants.MediaTypes.Guids.FolderGuid];
        }

        var ignoreUserStartNodes = await IgnoreUserStartNodes(dataTypeId);
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query,
            parentId,
            allowedMediaTypes,
            trashed,
            culture,
            skip,
            take,
            ignoreUserStartNodes);
        var result = new PagedModel<MediaItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMediaEntitySlim>().Select(_mediaPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return Ok(result);
    }

    private async Task<bool> IgnoreUserStartNodes(Guid? dataTypeKey) =>
        dataTypeKey is not null && await _dataTypeService.IsDataTypeIgnoringUserStartNodesAsync(dataTypeKey.Value);
}
