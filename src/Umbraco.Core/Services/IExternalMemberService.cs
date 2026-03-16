// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ExternalMemberService, which provides operations for managing
///     external-only members that are not backed by the content system.
/// </summary>
public interface IExternalMemberService
{
    /// <summary>
    ///     Gets an external member by its unique key.
    /// </summary>
    /// <param name="key">The unique identifier of the external member.</param>
    /// <returns>The <see cref="ExternalMemberIdentity"/> if found; otherwise <c>null</c>.</returns>
    Task<ExternalMemberIdentity?> GetByKeyAsync(Guid key);

    /// <summary>
    ///     Gets an external member by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The <see cref="ExternalMemberIdentity"/> if found; otherwise <c>null</c>.</returns>
    Task<ExternalMemberIdentity?> GetByEmailAsync(string email);

    /// <summary>
    ///     Gets an external member by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The <see cref="ExternalMemberIdentity"/> if found; otherwise <c>null</c>.</returns>
    Task<ExternalMemberIdentity?> GetByUsernameAsync(string username);

    /// <summary>
    ///     Gets a paged collection of all external members.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A <see cref="PagedModel{T}"/> containing the external members.</returns>
    Task<PagedModel<ExternalMemberIdentity>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Creates a new external member.
    /// </summary>
    /// <param name="member">The external member identity to create.</param>
    /// <param name="externalLogin">An optional external login to associate with the member in the same transaction.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the created member on success.</returns>
    Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> CreateAsync(ExternalMemberIdentity member, IExternalLogin? externalLogin = null);

    /// <summary>
    ///     Updates an existing external member.
    /// </summary>
    /// <param name="member">The external member identity to update.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the updated member on success.</returns>
    Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> UpdateAsync(ExternalMemberIdentity member);

    /// <summary>
    ///     Updates the login timestamp and security stamp for an external member.
    ///     This is a fast path with no entity load, no notifications, and no uniqueness checks.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <param name="lastLoginDate">The date and time of the login.</param>
    /// <param name="securityStamp">The new security stamp value.</param>
    Task UpdateLoginTimestampAsync(Guid memberKey, DateTime lastLoginDate, string securityStamp);

    /// <summary>
    ///     Deletes an external member by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the external member to delete.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the deleted member on success, or a not-found status.</returns>
    Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    ///     Gets the role names assigned to an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <returns>A collection of role names.</returns>
    Task<IEnumerable<string>> GetRolesAsync(Guid memberKey);

    /// <summary>
    ///     Assigns roles to an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <param name="roleNames">The names of the roles to assign.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> indicating the operation result.</returns>
    Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> AssignRolesAsync(Guid memberKey, string[] roleNames);

    /// <summary>
    ///     Removes roles from an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <param name="roleNames">The names of the roles to remove.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> indicating the operation result.</returns>
    Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> RemoveRolesAsync(Guid memberKey, string[] roleNames);

    /// <summary>
    ///     Converts an external-only member to a full content-based member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member to convert.</param>
    /// <param name="memberTypeAlias">The alias of the member type to use for the content-based member.</param>
    /// <param name="mapProfileData">
    ///     An optional callback invoked after the content member is created but before it is saved.
    ///     Receives the new <see cref="IMember"/> and the external member's <c>profileData</c> JSON string,
    ///     allowing the developer to map profile data fields to content properties
    ///     (e.g. <c>member.SetValue("department", ...)</c>).
    /// </param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the newly created <see cref="IMember"/> on success.</returns>
    Task<Attempt<IMember?, ExternalMemberOperationStatus>> ConvertToContentMemberAsync(Guid memberKey, string memberTypeAlias, Action<IMember, string?>? mapProfileData = null);
}
