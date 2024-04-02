using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentVersionService
{
    /// <summary>
    ///     Removes historic content versions according to a policy.
    /// </summary>
    IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate);

    /// <summary>
    ///     Gets paginated content versions for given content id paginated.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="culture" /> is invalid.</exception>
    [Obsolete("Use the async version instead. Scheduled to be removed in v15")]
    IEnumerable<ContentVersionMeta>? GetPagedContentVersions(int contentId, long pageIndex, int pageSize, out long totalRecords, string? culture = null);

    /// <summary>
    ///     Updates preventCleanup value for given content version.
    /// </summary>
    [Obsolete("Use the async version instead. Scheduled to be removed in v15")]
    void SetPreventCleanup(int versionId, bool preventCleanup, int userId = -1);

    ContentVersionMeta? Get(int versionId);
    Task<Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>> GetPagedContentVersionsAsync(Guid contentId, string? culture, int skip, int take);
    Task<Attempt<IContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId);

    Task<Attempt<ContentVersionOperationStatus>> SetPreventCleanupAsync(Guid versionId, bool preventCleanup, Guid userKey);
    Task<Attempt<ContentVersionOperationStatus>> RollBackAsync(Guid versionId, string? culture, Guid userKey);
}
