using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Retrieves paged references to a member, handling the external member fallback
/// when the entity-based lookup fails (external members have no <c>umbracoNode</c> entry).
/// </summary>
internal sealed class MemberReferenceService : IMemberReferenceService
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IMemberEditingService _memberEditingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberReferenceService"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service used to manage tracked references.</param>
    /// <param name="memberEditingService">Service used for member editing operations.</param>
    public MemberReferenceService(
        ITrackedReferencesService trackedReferencesService,
        IMemberEditingService memberEditingService)
    {
        _trackedReferencesService = trackedReferencesService;
        _memberEditingService = memberEditingService;
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedReferencesAsync(Guid id, int skip, int take)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result =
            await _trackedReferencesService.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, skip, take, true);

        if (result.Success)
        {
            return result;
        }

        // The entity-based lookup fails for external-only members (no umbracoNode entry).
        // Fall back to a key-based relation query if this is an external member.
        if (result.Status == GetReferencesOperationStatus.ContentNotFound
            && await _memberEditingService.IsExternalMemberAsync(id))
        {
#pragma warning disable CS0618 // Type or member is obsolete — using the key-based overload that doesn't require an entity.
            PagedModel<RelationItemModel> externalRelations = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, skip, take, true);
#pragma warning restore CS0618

            return Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, externalRelations);
        }

        return result;
    }
}
