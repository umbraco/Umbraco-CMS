using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="IAuditEntry"/> entities.
    /// </summary>
    public interface IAsyncAuditEntryRepository : IAuditEntryRepository
    {
        /// <summary>
        /// Gets a page of entries.
        /// </summary>
        Task<(IEnumerable<IAuditEntry> Entries,long RecordCount)> GetPageAsync(long pageIndex, int pageCount);
    }
}
