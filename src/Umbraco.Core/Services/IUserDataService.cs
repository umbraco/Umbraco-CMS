using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the user data service, which provides operations for managing user-specific data entries.
/// </summary>
public interface IUserDataService
{
    /// <summary>
    ///     Gets user data by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the user data.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IUserData" /> if found; otherwise, null.</returns>
    public Task<IUserData?> GetAsync(Guid key);

    /// <summary>
    ///     Gets a paged collection of user data with optional filtering.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="filter">Optional filter to apply to the query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PagedModel{T}" /> of <see cref="IUserData" />.</returns>
    public Task<PagedModel<IUserData>> GetAsync(
        int skip,
        int take,
        IUserDataFilter? filter = null);

    /// <summary>
    ///     Creates a new user data entry.
    /// </summary>
    /// <param name="userData">The <see cref="IUserData" /> to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the created user data and operation status.</returns>
    public Task<Attempt<IUserData, UserDataOperationStatus>> CreateAsync(IUserData userData);

    /// <summary>
    ///     Updates an existing user data entry.
    /// </summary>
    /// <param name="userData">The <see cref="IUserData" /> to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the updated user data and operation status.</returns>
    public Task<Attempt<IUserData, UserDataOperationStatus>> UpdateAsync(IUserData userData);

    /// <summary>
    ///     Deletes a user data entry by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the user data to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TStatus}" /> with the operation status.</returns>
    public Task<Attempt<UserDataOperationStatus>> DeleteAsync(Guid key);
}
