using Umbraco.Cms.Core.Models;

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
    IEnumerable<ContentVersionMeta>? GetPagedContentVersions(int contentId, long pageIndex, int pageSize, out long totalRecords, string? culture = null);

    /// <summary>
    ///     Updates preventCleanup value for given content version.
    /// </summary>
    void SetPreventCleanup(int versionId, bool preventCleanup, int userId = -1);
}
