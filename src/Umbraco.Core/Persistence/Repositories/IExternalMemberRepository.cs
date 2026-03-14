// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the repository for external-only members that are not backed by the content system.
/// </summary>
public interface IExternalMemberRepository
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
    ///     Gets a paged collection of external members.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A <see cref="PagedModel{T}"/> containing the external members.</returns>
    Task<PagedModel<ExternalMemberIdentity>> GetPagedAsync(int skip, int take);

    /// <summary>
    ///     Creates a new external member in the database.
    /// </summary>
    /// <param name="member">The external member identity to create.</param>
    /// <returns>The database identity of the created external member.</returns>
    Task<int> CreateAsync(ExternalMemberIdentity member);

    /// <summary>
    ///     Updates an existing external member in the database.
    /// </summary>
    /// <param name="member">The external member identity to update.</param>
    Task UpdateAsync(ExternalMemberIdentity member);

    /// <summary>
    ///     Updates the login timestamp and security stamp for an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <param name="lastLoginDate">The date and time of the login.</param>
    /// <param name="securityStamp">The new security stamp value.</param>
    Task UpdateLoginTimestampAsync(Guid memberKey, DateTime lastLoginDate, string securityStamp);

    /// <summary>
    ///     Deletes an external member by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the external member to delete.</param>
    Task DeleteAsync(Guid key);

    /// <summary>
    ///     Gets the role names assigned to an external member.
    /// </summary>
    /// <param name="memberKey">The unique key of the external member.</param>
    /// <returns>A collection of role names.</returns>
    Task<IEnumerable<string>> GetRolesAsync(Guid memberKey);

    /// <summary>
    ///     Assigns roles to an external member by database identities.
    /// </summary>
    /// <param name="externalMemberId">The database identity of the external member.</param>
    /// <param name="memberGroupIds">The database identities of the member groups to assign.</param>
    Task AssignRolesAsync(int externalMemberId, int[] memberGroupIds);

    /// <summary>
    ///     Removes roles from an external member by database identities.
    /// </summary>
    /// <param name="externalMemberId">The database identity of the external member.</param>
    /// <param name="memberGroupIds">The database identities of the member groups to remove.</param>
    Task RemoveRolesAsync(int externalMemberId, int[] memberGroupIds);
}
