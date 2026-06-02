using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IAuditEntry" /> entities.
/// </summary>
public interface IAuditEntryRepository : IAsyncReadWriteRepository<int, IAuditEntry>
{
    /// <summary>
    ///     Gets a page of entries.
    /// </summary>
    Task<PagedModel<IAuditEntry>> GetPageAsync(long pageIndex, int pageCount);
}
