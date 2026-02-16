using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory for creating member presentation (view) models from domain models.
/// </summary>
public interface IMemberPresentationFactory
{
    /// <summary>
    /// Creates a member response model from the specified member.
    /// </summary>
    /// <param name="member">The member to create the response model from.</param>
    /// <param name="currentUser">The current backoffice user.</param>
    /// <returns>A task that represents the asynchronous operation, containing the member response model.</returns>
    Task<MemberResponseModel> CreateResponseModelAsync(IMember member, IUser currentUser);

    /// <summary>
    /// Creates multiple member response models from the specified members.
    /// </summary>
    /// <param name="members">The members to create response models from.</param>
    /// <param name="currentUser">The current backoffice user.</param>
    /// <returns>A task that represents the asynchronous operation, containing the member response models.</returns>
    Task<IEnumerable<MemberResponseModel>> CreateMultipleAsync(IEnumerable<IMember> members, IUser currentUser);

    /// <summary>
    /// Creates a member item response model from the specified entity.
    /// </summary>
    /// <param name="entity">The member entity.</param>
    /// <returns>The member item response model.</returns>
    MemberItemResponseModel CreateItemResponseModel(IMemberEntitySlim entity);

    /// <summary>
    /// Creates a member item response model from the specified member.
    /// </summary>
    /// <param name="entity">The member.</param>
    /// <returns>The member item response model.</returns>
    MemberItemResponseModel CreateItemResponseModel(IMember entity);

    /// <summary>
    /// Creates a search item response model for a member entity, including ancestor breadcrumbs.
    /// </summary>
    // TODO (V18): Remove default implementation.
    SearchMemberItemResponseModel CreateSearchItemResponseModel(IMemberEntitySlim entity, IEnumerable<SearchResultAncestorModel> ancestors)
    {
        MemberItemResponseModel baseModel = CreateItemResponseModel(entity);
        return new SearchMemberItemResponseModel
        {
            Id = baseModel.Id,
            MemberType = baseModel.MemberType,
            Variants = baseModel.Variants,
            Kind = baseModel.Kind,
            Flags = baseModel.Flags,
            Ancestors = ancestors,
        };
    }
}
