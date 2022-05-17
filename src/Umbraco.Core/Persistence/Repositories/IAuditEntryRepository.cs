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

    /// <summary>
    ///     Determines whether the repository is available.
    /// </summary>
    /// <remarks>During an upgrade, the repository may not be available, until the table has been created.</remarks>
    bool IsAvailable();
}
