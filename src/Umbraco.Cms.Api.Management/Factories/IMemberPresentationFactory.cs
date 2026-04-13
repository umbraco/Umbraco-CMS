using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models of members.
/// </summary>
public interface IMemberPresentationFactory
{
    /// <summary>
    /// Creates a response model for the specified member asynchronously.
    /// </summary>
    /// <param name="member">The member to create the response model for.</param>
    /// <param name="currentUser">The current user performing the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the member response model.</returns>
    Task<MemberResponseModel> CreateResponseModelAsync(IMember member, IUser currentUser);

    /// <summary>
    /// Creates multiple member response models asynchronously from the given members.
    /// </summary>
    /// <param name="members">The collection of members to create response models for.</param>
    /// <param name="currentUser">The current user context for permission and personalization.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of member response models.</returns>
    Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members, IUser currentUser);

    /// <summary>
    /// Creates a response model for a member item based on the provided member entity.
    /// </summary>
    /// <param name="entity">The member entity slim instance to create the response model from.</param>
    /// <returns>A <see cref="MemberItemResponseModel"/> representing the member item response.</returns>
    MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity);

    /// <summary>
    /// Creates a response model for the given member entity.
    /// </summary>
    /// <param name="entity">The member entity to create the response model from.</param>
    /// <returns>A MemberItemResponseModel representing the member entity.</returns>
    MemberItemResponseModel CreateItemResponseModel(IMember entity);
}
