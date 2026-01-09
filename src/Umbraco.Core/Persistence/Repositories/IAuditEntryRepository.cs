using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IAuditEntry" /> entities.
/// </summary>
public interface IAuditEntryRepository : IReadWriteQueryRepository<int, IAuditEntry>
{
    /// <summary>
    ///     Gets a page of entries.
    /// </summary>
    IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records);
}
