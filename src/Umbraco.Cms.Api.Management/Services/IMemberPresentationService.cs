using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Service for resolving members across both content and external stores and creating presentation models.
/// </summary>
public interface IMemberPresentationService
{
    /// <summary>
    /// Resolves a member by key from either the content or external store and creates a response model.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <param name="currentUser">The current backoffice user performing the operation.</param>
    /// <returns>A <see cref="MemberResponseModel"/> if found; otherwise <c>null</c>.</returns>
    Task<MemberResponseModel?> CreateResponseModelByKeyAsync(Guid id, IUser currentUser);

    /// <summary>
    /// Resolves members by keys from both the content and external stores and creates item response models.
    /// </summary>
    /// <param name="ids">The unique identifiers of the members to resolve.</param>
    /// <returns>A collection of <see cref="MemberItemResponseModel"/> for all resolved members.</returns>
    Task<IEnumerable<MemberItemResponseModel>> CreateItemResponseModelsAsync(HashSet<Guid> ids);
}
