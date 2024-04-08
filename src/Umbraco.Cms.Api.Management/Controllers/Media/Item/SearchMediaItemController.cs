using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class SearchMediaItemController : MediaItemControllerBase
{
    private readonly IExamineEntitySearchService _examineEntitySearchService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    public SearchMediaItemController(IExamineEntitySearchService examineEntitySearchService, IMediaPresentationFactory mediaPresentationFactory)
    {
        _examineEntitySearchService = examineEntitySearchService;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _examineEntitySearchService.Search(UmbracoObjectTypes.Media, query, skip, take);
        var result = new PagedModel<MediaItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMediaEntitySlim>().Select(_mediaPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total
        };

        return await Task.FromResult(Ok(result));
    }
}
