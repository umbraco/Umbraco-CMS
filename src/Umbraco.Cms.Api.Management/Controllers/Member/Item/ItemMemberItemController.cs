using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiVersion("1.0")]
public class ItemMemberItemController : MemberItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public ItemMemberItemController(IEntityService entityService, IMemberPresentationFactory memberPresentationFactory)
    {
        _entityService = entityService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MemberItemResponseModel>());
        }

        IEnumerable<IMemberEntitySlim> members = _entityService
            .GetAll(UmbracoObjectTypes.Member, ids.ToArray())
            .OfType<IMemberEntitySlim>();

        IEnumerable<MemberItemResponseModel> responseModels = members.Select(_memberPresentationFactory.CreateItemResponseModel);
        return await Task.FromResult(Ok(responseModels));
    }
}
