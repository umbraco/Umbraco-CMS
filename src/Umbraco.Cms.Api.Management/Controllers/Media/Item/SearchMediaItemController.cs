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

[ApiVersion("1.0")]
public class SearchMediaItemController : MediaItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IDataTypeService _dataTypeService;

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

    [Obsolete("Use the non-obsolete constructor instead, will be removed in Umbraco 18.")]
    public SearchMediaItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(
            indexedEntitySearchService,
            mediaPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeService>())
    {
    }

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
    [ProducesResponseType(typeof(PagedModel<MediaItemResponseModel>), StatusCodes.Status200OK)]
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
