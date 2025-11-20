using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentVersionService
{
    /// <summary>
    ///     Removes historic content versions according to a policy.
    /// </summary>
    IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate);

    ContentVersionMeta? Get(int versionId);
    Task<Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>> GetPagedContentVersionsAsync(Guid contentId, string? culture, int skip, int take);
    Task<Attempt<IContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId);

    Task<Attempt<ContentVersionOperationStatus>> SetPreventCleanupAsync(Guid versionId, bool preventCleanup, Guid userKey);
    Task<Attempt<ContentVersionOperationStatus>> RollBackAsync(Guid versionId, string? culture, Guid userKey);
}
