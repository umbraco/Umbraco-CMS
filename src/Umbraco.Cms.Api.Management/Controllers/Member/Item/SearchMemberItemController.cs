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
    private readonly IExamineEntitySearchService _examineEntitySearchService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public SearchMemberItemController(IExamineEntitySearchService examineEntitySearchService, IMemberPresentationFactory memberPresentationFactory)
    {
        _examineEntitySearchService = examineEntitySearchService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Search(string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _examineEntitySearchService.Search(UmbracoObjectTypes.Member, query, skip, take);
        var result = new PagedModel<MemberItemResponseModel>
        {
            Items = searchResult.Items.OfType<IMemberEntitySlim>().Select(_memberPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total
        };

        return await Task.FromResult(Ok(result));
    }
}
