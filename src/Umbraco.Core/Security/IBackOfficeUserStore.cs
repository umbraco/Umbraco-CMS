using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Manages persistence of users.
/// </summary>
public interface IBackOfficeUserStore
{
    /// <summary>
    /// Saves an <see cref="IUser" />
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to Save</param>
    /// <returns>A task resolving into an <see cref="UserOperationStatus"/>.</returns>
    Task<UserOperationStatus> SaveAsync(IUser user);

    /// <summary>
    ///     Disables an <see cref="IUser" />
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to disable.</param>
    /// <returns>A task resolving into an <see cref="UserOperationStatus"/>.</returns>
    Task<UserOperationStatus> DisableAsync(IUser user);

    /// <summary>
    ///     Get an <see cref="IUser" /> by username
    /// </summary>
    /// <param name="username">Username to use for retrieval.</param>
    /// <returns>
    ///     A task resolving into an <see cref="IUser" />
    /// </returns>
    Task<IUser?> GetByUserNameAsync(string username);

    /// <summary>
    ///     Get an <see cref="IUser" /> by email
    /// </summary>
    /// <param name="email">Email to use for retrieval.</param>
    /// <returns>
    ///     A task resolving into an <see cref="IUser" />
    /// </returns>
    Task<IUser?> GetByEmailAsync(string email);

    /// <summary>
    ///     Gets a user by Id
    /// </summary>
    /// <param name="id">Id of the user to retrieve</param>
    /// <returns>
    ///     A task resolving into an <see cref="IUser" />
    /// </returns>
    Task<IUser?> GetAsync(int id);

    /// <summary>
    ///     Gets a user by it's key.
    /// </summary>
    /// <param name="key">Key of the user to retrieve.</param>
    /// <returns>Task resolving into an <see cref="IUser"/>.</returns>
    Task<IUser?> GetAsync(Guid key);

    Task<IEnumerable<IUser>> GetUsersAsync(params Guid[]? keys);

    Task<IEnumerable<IUser>> GetUsersAsync(params int[]? ids);


    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group.</param>
    /// <returns>
    ///     A task resolving into an  <see cref="IEnumerable{IUser}" />
    /// </returns>
    Task<IEnumerable<IUser>> GetAllInGroupAsync(int groupId);

}
