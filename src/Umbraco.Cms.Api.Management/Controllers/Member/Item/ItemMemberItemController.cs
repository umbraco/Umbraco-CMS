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

    /// <summary>
    /// API controller responsible for managing individual member items within the Umbraco CMS.
    /// Provides endpoints for retrieving, updating, or deleting member item details.
    /// </summary>
[ApiVersion("1.0")]
public class ItemMemberItemController : MemberItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

/// <summary>
/// Initializes a new instance of the <see cref="ItemMemberItemController"/> class, which manages member item operations in the API.
/// </summary>
/// <param name="entityService">Service used for entity operations and retrieval.</param>
/// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    public ItemMemberItemController(IEntityService entityService, IMemberPresentationFactory memberPresentationFactory)
    {
        _entityService = entityService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member items.")]
    [EndpointDescription("Gets a collection of member items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<MemberItemResponseModel>()));
        }

        IEnumerable<IMemberEntitySlim> members = _entityService
            .GetAll(UmbracoObjectTypes.Member, ids.ToArray())
            .OfType<IMemberEntitySlim>();

        IEnumerable<MemberItemResponseModel> responseModels = members.Select(_memberPresentationFactory.CreateItemResponseModel);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
