using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiVersion("1.0")]
public class SearchMemberItemController : MemberItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public SearchMemberItemController(IIndexedEntitySearchService indexedEntitySearchService, IMemberPresentationFactory memberPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [NonAction]
    [Obsolete("Scheduled to be removed in v16, use the non obsoleted method instead")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
        => await SearchWithAllowedTypes(cancellationToken, query, skip, take);

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchWithAllowedTypes(CancellationToken cancellationToken, string query, int skip = 0, int take = 100, [FromQuery]IEnumerable<Guid>? allowedMemberTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = _indexedEntitySearchService.Search(UmbracoObjectTypes.Member, query, null, allowedMemberTypes, skip, take);
        var result = new PagedModel<MemberItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMemberEntitySlim>().Select(_memberPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total
        };

        return await Task.FromResult(Ok(result));
    }
}
