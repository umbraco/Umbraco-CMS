using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the MemberGroupService, which provides operations for managing <see cref="IMemberGroup"/> objects.
/// </summary>
public interface IMemberGroupService : IService
{
    /// <summary>
    ///     Gets a member group by its name.
    /// </summary>
    /// <param name="name">The name of the member group.</param>
    /// <returns>The <see cref="IMemberGroup"/> if found; otherwise, <c>null</c>.</returns>
    IMemberGroup? GetByName(string? name);

    /// <summary>
    ///     Get a member group by name.
    /// </summary>
    /// <param name="name">Name of the member group to get.</param>
    /// <returns>A <see cref="IMemberGroup" /> object.</returns>
    Task<IMemberGroup?> GetByNameAsync(string name);

    /// <summary>
    ///     Get a member group by key.
    /// </summary>
    /// <param name="key"><see cref="Guid" /> of the member group to get.</param>
    /// <returns>A <see cref="IMemberGroup" /> object.</returns>
    Task<IMemberGroup?> GetAsync(Guid key);

    /// <summary>
    ///     Gets member groups by their keys.
    /// </summary>
    /// <param name="keys">The keys of the member groups to get.</param>
    /// <returns>An enumerable list of matching <see cref="IMemberGroup" /> objects.</returns>
    /// <remarks>
    ///     The default implementation fetches all groups and filters in-memory. Implementations
    ///     backed by a query-capable store may provide a more efficient override.
    /// </remarks>
    Task<IEnumerable<IMemberGroup>> GetAsync(IEnumerable<Guid> keys);

    /// <summary>
    ///     Gets all member groups
    /// </summary>
    /// <returns>An enumerable list of <see cref="IMemberGroup" /> objects.</returns>
    Task<IEnumerable<IMemberGroup>> GetAllAsync();

    /// <summary>
    ///     Gets a list of member groups with the given ids.
    /// </summary>
    /// <param name="ids">An enumerable list of <see cref="int" /> ids, to get the member groups by.</param>
    /// <returns>An enumerable list of <see cref="IMemberGroup" /> objects.</returns>
    Task<IEnumerable<IMemberGroup>> GetByIdsAsync(IEnumerable<int> ids);

    /// <summary>
    ///     Creates a new <see cref="IMemberGroup" /> object
    /// </summary>
    /// <param name="memberGroup"><see cref="IMemberGroup" /> to create</param>
    /// <returns>An attempt with a status of whether the operation was successful or not, and the created object if it succeeded.</returns>
    Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> CreateAsync(IMemberGroup memberGroup);

    /// <summary>
    ///     Deletes a <see cref="IMemberGroup" /> by removing it and its usages from the db
    /// </summary>
    /// <param name="key">The key of the <see cref="IMemberGroup" /> to delete</param>
    /// <returns>An attempt with a status of whether the operation was successful or not, and the deleted object if it succeeded.</returns>
    Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    ///     Updates <see cref="IMemberGroup" /> object
    /// </summary>
    /// <param name="memberGroup"><see cref="IMemberGroup" /> to create</param>
    /// <returns>An attempt with a status of whether the operation was successful or not, and the object.</returns>
    Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> UpdateAsync(IMemberGroup memberGroup);
}
