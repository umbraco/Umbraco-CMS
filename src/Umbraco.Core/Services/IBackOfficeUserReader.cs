using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides shared read access to back office users for both <see cref="IUserService"/> and <see cref="Security.IBackOfficeUserStore"/>.
/// </summary>
/// <remarks>
/// Centralises the repository calls so the two consumers cannot drift. Registered independently of
/// <see cref="Security.IBackOfficeUserStore"/> so it is available in delivery-only setups.
/// </remarks>
public interface IBackOfficeUserReader
{
    /// <summary>
    /// Retrieves a back office user by their integer identifier, falling back to a minimal upgrade-safe query
    /// if the user table schema is mid-migration.
    /// </summary>
    /// <param name="id">The integer identifier of the user.</param>
    /// <returns>The <see cref="IUser"/> if found; otherwise, <c>null</c>.</returns>
    IUser? GetById(int id);

    /// <summary>
    /// Retrieves a back office user by their unique key.
    /// </summary>
    /// <param name="key">The unique key of the user.</param>
    /// <returns>The <see cref="IUser"/> if found; otherwise, <c>null</c>.</returns>
    IUser? GetByKey(Guid key);

    /// <summary>
    /// Retrieves the back office users with the specified integer identifiers.
    /// </summary>
    /// <param name="ids">The integer identifiers of the users to retrieve.</param>
    /// <returns>The matching users, or an empty collection if <paramref name="ids"/> is empty.</returns>
    IEnumerable<IUser> GetManyById(IEnumerable<int> ids);

    /// <summary>
    /// Retrieves the back office users with the specified unique keys.
    /// </summary>
    /// <param name="keys">The unique keys of the users to retrieve.</param>
    /// <returns>The matching users, or an empty collection if <paramref name="keys"/> is empty.</returns>
    IEnumerable<IUser> GetManyByKey(IEnumerable<Guid> keys);

    /// <summary>
    /// Retrieves all back office users that belong to the specified user group.
    /// </summary>
    /// <param name="groupId">The integer identifier of the user group.</param>
    /// <returns>The users in the group, or an empty collection if the group has no members.</returns>
    IEnumerable<IUser> GetAllInGroup(int groupId);
}
