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

/// <summary>
/// Provides API endpoints for searching document items within the management interface.
/// </summary>
[ApiVersion("1.0")]
public class SearchDocumentItemController : DocumentItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDocumentItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service for searching indexed entities.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDocumentItemController"/> class.
    /// </summary>
    /// <param name="indexedEntitySearchService">The service used to perform searches on indexed entities. This dependency is injected.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models. This dependency is injected.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 18.")]
    public SearchDocumentItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IDocumentPresentationFactory documentPresentationFactory)
        : this(
            indexedEntitySearchService,
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeService>())
    {
    }

    /// <summary>
    /// Searches for document items, including those in the recycle bin, using the specified query and filters.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="trashed">If set, filters results to include only trashed items, only non-trashed items, or both (when null).</param>
    /// <param name="culture">An optional culture code to filter search results.</param>
    /// <param name="skip">The number of items to skip (for pagination).</param>
    /// <param name="take">The maximum number of items to return (for pagination).</param>
    /// <param name="parentId">An optional parent ID to filter results by parent.</param>
    /// <param name="allowedDocumentTypes">An optional list of allowed document type IDs to filter results.</param>
    /// <returns>A task representing the asynchronous operation, with an action result containing the search results.</returns>
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

    /// <summary>
    /// Searches for document items, including those in the recycle bin, based on the specified query and filters.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="trashed">Whether to include trashed (recycled) items in the search. Optional.</param>
    /// <param name="culture">The culture to filter the search results by. Optional.</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The number of items to return for paging.</param>
    /// <param name="parentId">The parent ID to filter the search results by. Optional.</param>
    /// <param name="allowedDocumentTypes">A list of allowed document type IDs to filter the search results by. Optional.</param>
    /// <param name="dataTypeId">The data type ID to filter the search results by. Optional.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains an <see cref="IActionResult"/> with the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches document items.")]
    [EndpointDescription("Searches document items by the provided query with pagination support.")]
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
