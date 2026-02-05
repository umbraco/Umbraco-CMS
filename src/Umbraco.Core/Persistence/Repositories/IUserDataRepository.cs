using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IUserData" /> entities.
/// </summary>
public interface IUserDataRepository
{
    /// <summary>
    ///     Gets user data by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the user data.</param>
    /// <returns>The user data if found; otherwise, <c>null</c>.</returns>
    Task<IUserData?> GetAsync(Guid key);

    /// <summary>
    ///     Gets paged user data.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="filter">An optional filter to apply.</param>
    /// <returns>A paged model of user data.</returns>
    Task<PagedModel<IUserData>> GetAsync(int skip, int take, IUserDataFilter? filter = null);

    /// <summary>
    ///     Saves new user data.
    /// </summary>
    /// <param name="userData">The user data to save.</param>
    /// <returns>The saved user data.</returns>
    Task<IUserData> Save(IUserData userData);

    /// <summary>
    ///     Updates existing user data.
    /// </summary>
    /// <param name="userData">The user data to update.</param>
    /// <returns>The updated user data.</returns>
    Task<IUserData> Update(IUserData userData);

    /// <summary>
    ///     Deletes user data.
    /// </summary>
    /// <param name="userData">The user data to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Delete(IUserData userData);
}
