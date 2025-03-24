using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class SearchDocumentItemController : DocumentItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public SearchDocumentItemController(IIndexedEntitySearchService indexedEntitySearchService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [NonAction]
    [Obsolete("Scheduled to be removed in v16, use the non obsoleted method instead")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
        => await SearchFromParent(cancellationToken, query, skip, take);

    [NonAction]
    [Obsolete("Scheduled to be removed in v16, use the non obsoleted method instead")]
    public async Task<IActionResult> SearchFromParent(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, Guid? parentId = null)
        => await SearchWithTrashed(cancellationToken, query, null, skip, take, parentId);

    [NonAction]
    [Obsolete("Scheduled to be removed in v16, use the non obsoleted method instead")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFromParentWithAllowedTypes(
        CancellationToken cancellationToken,
        string query,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedDocumentTypes = null) =>
        await SearchWithTrashed(cancellationToken, query, null, skip, take, parentId, allowedDocumentTypes);

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchWithTrashed(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedDocumentTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = _indexedEntitySearchService.Search(UmbracoObjectTypes.Document, query, parentId, allowedDocumentTypes, trashed, skip, take);
        var result = new PagedModel<DocumentItemResponseModel>
        {
            Items = searchResult.Items.OfType<IDocumentEntitySlim>().Select(_documentPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return await Task.FromResult(Ok(result));
    }
}
