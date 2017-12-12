using System.Collections.Generic;
using Semver;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Manages migration entries in the database
    /// </summary>
    public sealed class MigrationEntryService : ScopeRepositoryService, IMigrationEntryService
    {
        private readonly IMigrationEntryRepository _migrationEntryRepository;

        public MigrationEntryService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IMigrationEntryRepository migrationEntryRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _migrationEntryRepository = migrationEntryRepository;
        }

        /// <summary>
        /// Creates a migration entry, will throw an exception if it already exists
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public IMigrationEntry CreateEntry(string migrationName, SemVersion version)
        {
            var entry = new MigrationEntry
            {
                MigrationName = migrationName,
                Version = version
            };

            using (var scope = ScopeProvider.CreateScope())
            {
                _migrationEntryRepository.Save(entry);
                scope.Complete();
            }

            return entry;
        }

        /// <summary>
        /// Finds a migration by name and version, returns null if not found
        /// </summary>
        /// <param name="migrationName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public IMigrationEntry FindEntry(string migrationName, SemVersion version)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _migrationEntryRepository.FindEntry(migrationName, version);
            }
        }

        /// <summary>
        /// Gets all entries for a given migration name
        /// </summary>
        /// <param name="migrationName"></param>
        /// <returns></returns>
        public IEnumerable<IMigrationEntry> GetAll(string migrationName)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IMigrationEntry>()
                    .Where(x => x.MigrationName.ToUpper() == migrationName.ToUpper());
                return _migrationEntryRepository.Get(query);
            }
        }

    }
}
