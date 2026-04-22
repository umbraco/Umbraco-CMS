using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Resolves members across both the content and external member stores and creates presentation models.
/// </summary>
internal sealed class MemberPresentationService : IMemberPresentationService
{
    private readonly IEntityService _entityService;
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberPresentationService"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations and retrieval.</param>
    /// <param name="memberEditingService">Service used for member editing operations.</param>
    /// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    public MemberPresentationService(
        IEntityService entityService,
        IMemberEditingService memberEditingService,
        IMemberPresentationFactory memberPresentationFactory)
    {
        _entityService = entityService;
        _memberEditingService = memberEditingService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    /// <inheritdoc/>
    public async Task<MemberResponseModel?> CreateResponseModelByKeyAsync(Guid id, IUser currentUser)
    {
        IMember? member = await _memberEditingService.GetAsync(id);
        if (member is not null)
        {
            return await _memberPresentationFactory.CreateResponseModelAsync(member, currentUser);
        }

        ExternalMemberIdentity? externalMember = await _memberEditingService.GetExternalMemberAsync(id);
        if (externalMember is not null)
        {
            return await _memberPresentationFactory.CreateExternalMemberResponseModelAsync(externalMember);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MemberItemResponseModel>> CreateItemResponseModelsAsync(HashSet<Guid> ids)
    {
        IMemberEntitySlim[] contentMembers = _entityService
            .GetAll(UmbracoObjectTypes.Member, ids.ToArray())
            .OfType<IMemberEntitySlim>()
            .ToArray();

        var responseModels = new List<MemberItemResponseModel>(
            contentMembers.Select(_memberPresentationFactory.CreateItemResponseModel));

        var resolvedIds = contentMembers.Select(m => m.Key).ToHashSet();

        foreach (Guid unresolvedId in ids.Where(id => resolvedIds.Contains(id) is false))
        {
            ExternalMemberIdentity? externalMember = await _memberEditingService.GetExternalMemberAsync(unresolvedId);
            if (externalMember is not null)
            {
                responseModels.Add(_memberPresentationFactory.CreateExternalMemberItemResponseModel(externalMember));
            }
        }

        return responseModels;
    }
}
