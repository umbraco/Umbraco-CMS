using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class SearchDocumentItemController : DocumentItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IDataTypeService _dataTypeService;

    [ActivatorUtilitiesConstructor]
    public SearchDocumentItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDataTypeService dataTypeService)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _documentPresentationFactory = documentPresentationFactory;
        _dataTypeService = dataTypeService;
    }

    [Obsolete("Use the non-obsolete constructor instead, will be removed in v18")]
    public SearchDocumentItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IDocumentPresentationFactory documentPresentationFactory)
        : this(
            indexedEntitySearchService,
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeService>())
    {
    }

    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SearchWithTrashed(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedDocumentTypes = null)
        => await SearchWithTrashed(
            cancellationToken,
            query,
            trashed,
            culture,
            skip,
            take,
            parentId,
            allowedDocumentTypes,
            null);

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchWithTrashed(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedDocumentTypes = null,
        Guid? dataTypeId = null)
    {
        var ignoreUserStartNodes = await IgnoreUserStartNodes(dataTypeId);
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query,
            parentId,
            allowedDocumentTypes,
            trashed,
            culture,
            skip,
            take,
            ignoreUserStartNodes);

        var result = new PagedModel<DocumentItemResponseModel>
        {
            Items = searchResult.Items.OfType<IDocumentEntitySlim>().Select(_documentPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return Ok(result);
    }

    private async Task<bool> IgnoreUserStartNodes(Guid? dataTypeKey) =>
        dataTypeKey is not null && await _dataTypeService.IsDataTypeIgnoringUserStartNodesAsync(dataTypeKey.Value);
}
