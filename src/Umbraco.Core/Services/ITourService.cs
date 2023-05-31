using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ITourService
{
    /// <summary>
    /// Persists a <see cref="UserTourStatus"/> for a user.
    /// </summary>
    /// <param name="status">The status to persist.</param>
    /// <param name="userKey">The key of the user to persist it for.</param>
    /// <returns>An operation status specifying if the operation was successful.</returns>
    Task<TourOperationStatus> SetAsync(UserTourStatus status, Guid userKey);

    /// <summary>
    /// Gets all <see cref="UserTourStatus"/> for a user.
    /// </summary>
    /// <param name="userKey">The key of the user to get tour data for.</param>
    /// <returns>An attempt containing an enumerable of <see cref="UserTourStatus"/> and a status.</returns>
    Task<Attempt<IEnumerable<UserTourStatus>, TourOperationStatus>> GetAllAsync(Guid userKey);
}
