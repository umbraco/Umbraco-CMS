using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDocumentVersionRepository : IRepository
    {
        /// <summary>
        /// Gets a list of all historic content versions.
        /// </summary>
        public IReadOnlyCollection<HistoricContentVersionMeta> GetDocumentVersionsEligibleForCleanup();

        /// <summary>
        /// Gets cleanup policy override settings per content type.
        /// </summary>
        public IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies();

        /// <summary>
        /// Deletes multiple content versions by ID.
        /// </summary>
        void DeleteVersions(IEnumerable<int> versionIds);
    }
}
