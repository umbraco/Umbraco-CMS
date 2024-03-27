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
    private readonly IEntitySearchService _entitySearchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public SearchDocumentItemController(IEntitySearchService entitySearchService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _entitySearchService = entitySearchService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Search(string query, int skip = 0, int take = 100)
    {
        // FIXME: use Examine and handle user start nodes
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.Document, query, skip, take);
        var result = new PagedModel<DocumentItemResponseModel>
        {
            Items = searchResult.Items.OfType<IDocumentEntitySlim>().Select(_documentPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total
        };

        return await Task.FromResult(Ok(result));
    }
}
