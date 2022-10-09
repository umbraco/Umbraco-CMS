using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IUserRepository : IReadWriteQueryRepository<int, IUser>
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
    [Obsolete("This method will be removed in future versions.  Please use ExistsByUserName instead.")]
    bool Exists(string username);

    /// <summary>
    ///     Checks if a user with the username exists
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    bool ExistsByUserName(string username);

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

    IProfile? GetProfile(string username);

    IProfile? GetProfile(int id);

    IDictionary<UserState, int> GetUserStates();

    Guid CreateLoginSession(int? userId, string requestingIpAddress, bool cleanStaleSessions = true);

    bool ValidateLoginSession(int userId, Guid sessionId);

    int ClearLoginSessions(int userId);

    int ClearLoginSessions(TimeSpan timespan);

    void ClearLoginSession(Guid sessionId);

    IEnumerable<IUser> GetNextUsers(int id, int count);
}
