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
    ///     Updates only the login-related properties of an external member via a lightweight direct SQL update.
    /// </summary>
    /// <param name="member">The external member identity carrying the new values for <c>LastLoginDate</c> and <c>SecurityStamp</c>.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the updated member on success.</returns>
    /// <remarks>
    ///     Use this instead of <see cref="UpdateAsync"/> on the login path when only <c>LastLoginDate</c>
    ///     and/or <c>SecurityStamp</c> have changed. Skips the uniqueness checks and full-DTO mapping
    ///     performed by the full update, and deliberately does <em>not</em> bump <c>UpdateDate</c> —
    ///     login is not treated as a member update, and the Examine index is not refreshed. Any
    ///     change to real member data (name, email, profile data, etc.) must go through
    ///     <see cref="UpdateAsync"/> which does bump <c>UpdateDate</c> and triggers a re-index.
    /// </remarks>
    Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> UpdateLoginPropertiesAsync(ExternalMemberIdentity member);

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
    Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> AssignRolesAsync(Guid memberKey, string[] roleNames);

    /// <summary>
    ///     Removes roles from an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <param name="roleNames">The names of the roles to remove.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> indicating the operation result.</returns>
    Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> RemoveRolesAsync(Guid memberKey, string[] roleNames);

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

    /// <summary>
    ///     Validates whether <see cref="ConvertToContentMemberAsync"/> would succeed for the given member,
    ///     without mutating any data.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member to validate for conversion.</param>
    /// <param name="memberTypeAlias">The alias of the member type the content member would use.</param>
    /// <returns>
    ///     <see cref="ExternalMemberOperationStatus.Success"/> if the conversion would succeed; otherwise
    ///     the status that would cause it to fail (<see cref="ExternalMemberOperationStatus.NotFound"/>,
    ///     <see cref="ExternalMemberOperationStatus.InvalidMemberType"/>,
    ///     <see cref="ExternalMemberOperationStatus.DuplicateUsername"/> or
    ///     <see cref="ExternalMemberOperationStatus.DuplicateEmail"/>).
    /// </returns>
    // TODO (V19): remove the default implementation.
    Task<ExternalMemberOperationStatus> ValidateConvertToContentMemberAsync(Guid memberKey, string memberTypeAlias)
        => Task.FromResult(ExternalMemberOperationStatus.NotImplemented);

    /// <summary>
    ///     Converts a full content-based member into an external-only (lightweight) member,
    ///     preserving its key, identity fields, group memberships and external login links.
    /// </summary>
    /// <param name="memberKey">The unique key of the content member to convert.</param>
    /// <param name="mapProfileData">
    ///     An optional callback invoked after the <see cref="ExternalMemberIdentity"/> is built from the
    ///     content member but before it is persisted. Receives the new identity and the source
    ///     <see cref="IMember"/>, allowing the developer to map content properties into the identity
    ///     (e.g. serialize values into <see cref="ExternalMemberIdentity.ProfileData"/>).
    /// </param>
    /// <param name="requireExternalLogin">
    ///     When <c>true</c> (the default) the conversion is rejected with
    ///     <see cref="ExternalMemberOperationStatus.NoExternalLogin"/> unless the member already has an
    ///     external login link — an external-only member has no password and can otherwise never
    ///     authenticate. Set to <c>false</c> to force the conversion of a password-only member, relying
    ///     on auto-linking to recreate a login link on the member's next external sign-in.
    /// </param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> with the created <see cref="ExternalMemberIdentity"/> on success.</returns>
    /// <remarks>
    ///     The conversion is <em>not</em> performed in a single transaction. The content member is
    ///     deleted and the external identity created in one scope; the captured login links and tokens
    ///     are re-saved in a second scope, because deleting a member queues a deferred notification that
    ///     removes its login links at the end of the first scope. On a partial failure between the two
    ///     scopes the member exists without a login link, which auto-linking recreates on the next
    ///     external sign-in. Do not call this method within an ambient scope, as that would defer the
    ///     login-link deletion past the re-save and leave the member with no link.
    /// </remarks>
    // TODO (V19): remove the default implementation.
    Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> ConvertToExternalMemberAsync(
        Guid memberKey,
        Action<ExternalMemberIdentity, IMember>? mapProfileData = null,
        bool requireExternalLogin = true)
        => Task.FromResult(Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(
            ExternalMemberOperationStatus.NotImplemented, null));

    /// <summary>
    ///     Validates whether <see cref="ConvertToExternalMemberAsync"/> would succeed for the given member,
    ///     without mutating any data.
    /// </summary>
    /// <param name="memberKey">The unique key of the content member to validate for conversion.</param>
    /// <param name="requireExternalLogin">
    ///     When <c>true</c> (the default) a member with no external login link yields
    ///     <see cref="ExternalMemberOperationStatus.NoExternalLogin"/>.
    /// </param>
    /// <returns>
    ///     <see cref="ExternalMemberOperationStatus.Success"/> if the conversion would succeed; otherwise
    ///     the status that would cause it to fail (<see cref="ExternalMemberOperationStatus.NotFound"/>,
    ///     <see cref="ExternalMemberOperationStatus.NoExternalLogin"/>,
    ///     <see cref="ExternalMemberOperationStatus.DuplicateUsername"/> or
    ///     <see cref="ExternalMemberOperationStatus.DuplicateEmail"/>).
    /// </returns>
    // TODO (V19): remove the default implementation.
    Task<ExternalMemberOperationStatus> ValidateConvertToExternalMemberAsync(Guid memberKey, bool requireExternalLogin = true)
        => Task.FromResult(ExternalMemberOperationStatus.NotImplemented);
}
