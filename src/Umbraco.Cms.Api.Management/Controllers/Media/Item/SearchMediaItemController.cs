using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class SearchMediaItemController : MediaItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IDataTypeService _dataTypeService;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchMediaItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMediaPresentationFactory mediaPresentationFactory,
        IDataTypeService dataTypeService,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _dataTypeService = dataTypeService;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchMediaItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMediaPresentationFactory mediaPresentationFactory,
        IDataTypeService dataTypeService)
        : this(
            indexedEntitySearchService,
            mediaPresentationFactory,
            dataTypeService,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

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

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchMediaItemResponseModel>), StatusCodes.Status200OK)]
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

        IMediaEntitySlim[] mediaEntities = searchResult.Items.OfType<IMediaEntitySlim>().ToArray();

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(mediaEntities, UmbracoObjectTypes.Media);

        SearchMediaItemResponseModel[] items = mediaEntities
            .Select(entity =>
                _mediaPresentationFactory.CreateSearchItemResponseModel(
                    entity,
                    ancestorsByKey.TryGetValue(entity.Key, out IReadOnlyList<SearchResultAncestorModel>? ancestors)
                        ? ancestors
                        : []))
            .ToArray();

        var result = new PagedModel<SearchMediaItemResponseModel>
        {
            Items = items,
            Total = searchResult.Total,
        };

        return Ok(result);
    }

    private async Task<bool> IgnoreUserStartNodes(Guid? dataTypeKey) =>
        dataTypeKey is not null && await _dataTypeService.IsDataTypeIgnoringUserStartNodesAsync(dataTypeKey.Value);
}
