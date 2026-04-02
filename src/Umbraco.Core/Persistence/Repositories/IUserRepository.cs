using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IUser" /> entities.
/// </summary>
public interface IUserRepository : IReadWriteQueryRepository<Guid, IUser>
{
    /// <summary>
    ///     Gets the count of items based on a complex query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    int GetCountByQuery(IQuery<IUser>? query);

    /// <summary>
    ///     Checks if a user with the username exists
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    bool ExistsByUserName(string username);

    /// <summary>
    ///     Returns a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    ///     A cached <see cref="IUser" /> instance
    /// </returns>
    IUser? Get(int id);

    /// <summary>
    ///     Checks if a user with the login exists
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    bool ExistsByLogin(string login);

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    IEnumerable<IUser> GetAllInGroup(int groupId);

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects not associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    IEnumerable<IUser> GetAllNotInGroup(int groupId);

    /// <summary>
    ///     Gets paged user results
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="includeUserGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="excludeUserGroups">
    ///     A filter to only include users that do not belong to these user groups
    /// </param>
    /// <param name="userState">Optional parameter to filter by specified user state</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<IUser> GetPagedResultsByQuery(
        IQuery<IUser>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Expression<Func<IUser, object?>> orderBy,
        Direction orderDirection = Direction.Ascending,
        string[]? includeUserGroups = null,
        string[]? excludeUserGroups = null,
        UserState[]? userState = null,
        IQuery<IUser>? filter = null);

    /// <summary>
    ///     Returns a user by username
    /// </summary>
    /// <param name="username"></param>
    /// <param name="includeSecurityData">
    ///     This is only used for a shim in order to upgrade to 7.7
    /// </param>
    /// <returns>
    ///     A non cached <see cref="IUser" /> instance
    /// </returns>
    IUser? GetByUsername(string username, bool includeSecurityData);

    /// <summary>
    /// Gets a user by username for upgrade purposes, this will only return a result if the current runtime state is upgrade.
    /// </summary>
    /// <remarks>
    /// This only resolves the minimum amount of fields required to authorize for an upgrade.
    /// We need this to be able to add new columns to the user table.
    /// </remarks>
    /// <param name="username">The username to find the user by.</param>
    /// <returns>An uncached <see cref="IUser"/> instance.</returns>
    IUser? GetForUpgradeByUsername(string username) => GetByUsername(username, false);

    /// <summary>
    /// Gets a user by email for upgrade purposes, this will only return a result if the current runtime state is upgrade.
    /// </summary>
    /// <remarks>
    /// This only resolves the minimum amount of fields required to authorize for an upgrade.
    /// We need this to be able to add new columns to the user table.
    /// </remarks>
    /// <param name="email">The email to find the user by.</param>
    /// <returns>An uncached <see cref="IUser"/> instance.</returns>
    IUser? GetForUpgradeByEmail(string email) => GetMany().FirstOrDefault(x=>x.Email == email);

    /// <summary>
    /// Gets a user for upgrade purposes, this will only return a result if the current runtime state is upgrade.
    /// </summary>
    /// <remarks>
    /// This only resolves the minimum amount of fields required to authorize for an upgrade.
    /// We need this to be able to add new columns to the user table.
    /// </remarks>
    /// <param name="id">The id to find the user by.</param>
    /// <returns>An uncached <see cref="IUser"/> instance.</returns>
    IUser? GetForUpgrade(int id) => Get(id, false);

    /// <summary>
    ///     Returns a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeSecurityData">
    ///     This is only used for a shim in order to upgrade to 7.7
    /// </param>
    /// <returns>
    ///     A non cached <see cref="IUser" /> instance
    /// </returns>
    IUser? Get(int? id, bool includeSecurityData);

    /// <summary>
    ///     Gets a user profile by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The user profile if found; otherwise, <c>null</c>.</returns>
    IProfile? GetProfile(string username);

    /// <summary>
    ///     Gets a user profile by identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The user profile if found; otherwise, <c>null</c>.</returns>
    IProfile? GetProfile(int id);

    /// <summary>
    ///     Gets the count of users grouped by their state.
    /// </summary>
    /// <returns>A dictionary mapping user states to their counts.</returns>
    IDictionary<UserState, int> GetUserStates();

    /// <summary>
    ///     Creates a login session for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="requestingIpAddress">The IP address of the requesting client.</param>
    /// <param name="cleanStaleSessions">Whether to clean stale sessions.</param>
    /// <returns>The unique identifier of the created session.</returns>
    Guid CreateLoginSession(int? userId, string requestingIpAddress, bool cleanStaleSessions = true);

    /// <summary>
    ///     Validates a login session for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns><c>true</c> if the session is valid; otherwise, <c>false</c>.</returns>
    bool ValidateLoginSession(int userId, Guid sessionId);

    /// <summary>
    ///     Clears all login sessions for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The number of sessions cleared.</returns>
    int ClearLoginSessions(int userId);

    /// <summary>
    ///     Clears login sessions older than the specified timespan.
    /// </summary>
    /// <param name="timespan">The timespan after which sessions are considered stale.</param>
    /// <returns>The number of sessions cleared.</returns>
    int ClearLoginSessions(TimeSpan timespan);

    /// <summary>
    ///     Clears a specific login session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    void ClearLoginSession(Guid sessionId);

    /// <summary>
    ///     Gets all client identifiers.
    /// </summary>
    /// <returns>A collection of client identifiers.</returns>
    IEnumerable<string> GetAllClientIds();

    /// <summary>
    ///     Gets all client identifiers for a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>A collection of client identifiers.</returns>
    IEnumerable<string> GetClientIds(int id);

    /// <summary>
    ///     Adds a client identifier for a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="clientId">The client identifier to add.</param>
    void AddClientId(int id, string clientId);

    /// <summary>
    ///     Removes a client identifier from a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="clientId">The client identifier to remove.</param>
    /// <returns><c>true</c> if the client identifier was removed; otherwise, <c>false</c>.</returns>
    bool RemoveClientId(int id, string clientId);

    /// <summary>
    ///     Gets a user by their client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The user if found; otherwise, <c>null</c>.</returns>
    IUser? GetByClientId(string clientId);

    /// <summary>
    ///     Invalidates sessions for users that aren't associated with the current collection of providers.
    /// </summary>
    /// <param name="currentLoginProviders">The names of the currently configured providers.</param>
    void InvalidateSessionsForRemovedProviders(IEnumerable<string> currentLoginProviders) { }
}
