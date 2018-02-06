using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="IAuditEntry"/> entities.
    /// </summary>
    public interface IAuditEntryRepository : IRepositoryQueryable<int, IAuditEntry>
    {
        /// <summary>
        /// Gets a page of entries.
        /// </summary>
        IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records);
    }
}
