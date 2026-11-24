using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for named draft branches (see <see cref="IContentDraftBranch"/>).
/// </summary>
public interface IContentDraftBranchRepository : IRepository
{
    /// <summary>
    /// Gets a draft branch by its id.
    /// </summary>
    IContentDraftBranch? Get(int id);

    /// <summary>
    /// Gets a draft branch by its key.
    /// </summary>
    IContentDraftBranch? Get(Guid key);

    /// <summary>
    /// Gets all draft branches for a given node, in creation order.
    /// </summary>
    IEnumerable<IContentDraftBranch> GetByNodeId(int nodeId);

    /// <summary>
    /// Creates or updates a draft branch.
    /// </summary>
    void Save(IContentDraftBranch branch);

    /// <summary>
    /// Permanently deletes a draft branch row. Used by both ordinary editorial abandonment and promotion -
    /// both are immediate, terminal deletes; there is no lingering terminal status and no cleanup job that
    /// reaps branches later.
    /// </summary>
    void Delete(int id);

    /// <summary>
    /// Permanently deletes multiple draft branch rows by id. See <see cref="Delete"/>.
    /// </summary>
    void DeleteByIds(IEnumerable<int> ids);

    /// <summary>
    /// Gets draft branches due for scheduled publishing: those whose
    /// <see cref="IContentDraftBranch.ScheduledPublishDate"/> is set and has passed.
    /// </summary>
    /// <param name="date">Branches scheduled at or before this date are returned.</param>
    IEnumerable<IContentDraftBranch> GetScheduledForPublish(DateTime date);
}
