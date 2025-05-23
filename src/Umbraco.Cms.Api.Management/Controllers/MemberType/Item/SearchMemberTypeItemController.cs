using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

[ApiVersion("1.0")]
public class SearchMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _mapper;

    public SearchMemberTypeItemController(IEntitySearchService entitySearchService, IMemberTypeService memberTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _memberTypeService = memberTypeService;
        _mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MemberType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Task.FromResult<IActionResult>(Ok(new PagedModel<MemberTypeItemResponseModel> { Total = searchResult.Total }));
        }

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray());
        var result = new PagedModel<MemberTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(memberTypes),
            Total = searchResult.Total
        };

        return Task.FromResult<IActionResult>(Ok(result));
    }
}
