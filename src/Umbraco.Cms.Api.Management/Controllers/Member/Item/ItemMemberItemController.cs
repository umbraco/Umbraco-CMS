using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
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
    private readonly IMemberEditingService _memberEditingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMemberItemController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations and retrieval.</param>
    /// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    /// <param name="memberEditingService">Service used for member editing operations.</param>
    [ActivatorUtilitiesConstructor]
    public ItemMemberItemController(
        IEntityService entityService,
        IMemberPresentationFactory memberPresentationFactory,
        IMemberEditingService memberEditingService)
    {
        _entityService = entityService;
        _memberPresentationFactory = memberPresentationFactory;
        _memberEditingService = memberEditingService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMemberItemController"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ItemMemberItemController(
        IEntityService entityService,
        IMemberPresentationFactory memberPresentationFactory)
        : this(
            entityService,
            memberPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IMemberEditingService>())
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of member items.")]
    [EndpointDescription("Gets a collection of member items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MemberItemResponseModel>());
        }

        // Resolve content members from the entity service.
        IMemberEntitySlim[] contentMembers = _entityService
            .GetAll(UmbracoObjectTypes.Member, ids.ToArray())
            .OfType<IMemberEntitySlim>()
            .ToArray();

        var responseModels = new List<MemberItemResponseModel>(
            contentMembers.Select(_memberPresentationFactory.CreateItemResponseModel));

        // Find any IDs not resolved from the content store and check external members.
        HashSet<Guid> resolvedIds = contentMembers.Select(m => m.Key).ToHashSet();
        IEnumerable<Guid> unresolvedIds = ids.Where(id => !resolvedIds.Contains(id));

        foreach (Guid unresolvedId in unresolvedIds)
        {
            ExternalMemberIdentity? externalMember = await _memberEditingService.GetExternalMemberAsync(unresolvedId);
            if (externalMember is not null)
            {
                responseModels.Add(_memberPresentationFactory.CreateExternalMemberItemResponseModel(externalMember));
            }
        }

        return Ok(responseModels);
    }
}
