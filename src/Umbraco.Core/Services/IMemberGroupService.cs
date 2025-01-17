using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IMemberGroupService : IService
{
    [Obsolete("Please use the asynchronous counterpart. Scheduled for removal in v15.")]
    IEnumerable<IMemberGroup> GetAll();

    [Obsolete("Please use Guid instead of Int id. Scheduled for removal in v15.")]
    IMemberGroup? GetById(int id);

    [Obsolete("Please use the asynchronous counterpart. Scheduled for removal in v15.")]
    IMemberGroup? GetById(Guid id);

    [Obsolete("Please use the asynchronous counterpart. Scheduled for removal in v15.")]
    IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids);

    IMemberGroup? GetByName(string? name);

    [Obsolete("Please use the respective CreateAsync/UpdateAsync for you save operations. Scheduled for removal in v15.")]
    void Save(IMemberGroup memberGroup);

    [Obsolete("Please use the asynchronous counterpart. Scheduled for removal in v15.")]
    void Delete(IMemberGroup memberGroup);

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
    Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    ///     Updates <see cref="IMemberGroup" /> object
    /// </summary>
    /// <param name="memberGroup"><see cref="IMemberGroup" /> to create</param>
    /// <returns>An attempt with a status of whether the operation was successful or not, and the object.</returns>
    Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> UpdateAsync(IMemberGroup memberGroup);
}
