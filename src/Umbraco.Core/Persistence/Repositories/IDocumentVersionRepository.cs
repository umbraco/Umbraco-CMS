using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDocumentVersionRepository : IRepository
    {
        /// <summary>
        /// Gets a list of all historic content versions.
        /// </summary>
        public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup();

        /// <summary>
        /// Gets cleanup policy override settings per content type.
        /// </summary>
        public IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies();

        /// <summary>
        /// Gets paginated content versions for given content id paginated.
        /// </summary>
        public IEnumerable<ContentVersionMeta> GetPagedItemsByContentId(int contentId, long pageIndex, int pageSize, out long totalRecords, int? languageId = null);

        /// <summary>
        /// Deletes multiple content versions by ID.
        /// </summary>
        void DeleteVersions(IEnumerable<int> versionIds);
    }
}
