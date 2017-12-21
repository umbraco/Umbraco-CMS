using System;
using System.Collections.Generic;
using System.Linq;
using Semver;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents the Migration Runner, which is used to apply migrations to
    /// the umbraco database.
    /// </summary>
    public class MigrationRunner
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationCollectionBuilder _migrationBuilder;
        private readonly ILogger _logger;

        private readonly Type[] _migrationTypes;
        private readonly IMigrationEntryService _migrationEntryService;
        private readonly SemVersion _currentVersion;
        private readonly SemVersion _targetVersion;
        private readonly string _productName;

        private MigrationRunner(IScopeProvider scopeProvider, IMigrationCollectionBuilder migrationBuilder, IMigrationEntryService migrationEntryService, ILogger logger, SemVersion currentVersion, SemVersion targetVersion, string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentNullOrEmptyException(nameof(productName));

            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            _migrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _migrationEntryService = migrationEntryService ?? throw new ArgumentNullException(nameof(migrationEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            _targetVersion = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));

            _productName = productName;
        }

        public MigrationRunner(IScopeProvider scopeProvider, IMigrationCollectionBuilder migrationBuilder, IMigrationEntryService migrationEntryService, ILogger logger, SemVersion currentVersion, SemVersion targetVersion, string productName, params Type[] migrations)
            : this(scopeProvider, migrationBuilder, migrationEntryService, logger, currentVersion, targetVersion, productName)
        {
            _migrationTypes = migrations == null || migrations.Length == 0 ? null : migrations;
        }

        /// <summary>
        /// Executes migrations.
        /// </summary>
        public bool Execute(bool upgrade = true)
        {
            _logger.Info<MigrationRunner>("Initializing database migrations");

            // get migrations to execute
            var mt = GetMigrationTypes();
            mt = upgrade
                ? OrderedUpgradeMigrations(mt)
                : OrderedDowngradeMigrations(mt);
            var migrationTypes = mt.ToList();

            if (Migrating.IsRaisedEventCancelled(new MigrationEventArgs(migrationTypes, _currentVersion, _targetVersion, _productName, true), this))
            {
                _logger.Warn<MigrationRunner>("Migration was cancelled by an event");
                return false;
            }

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var context = new MigrationContext(scope.Database, _logger);
                    var migrations = migrationTypes.Select(x => _migrationBuilder.Instanciate(x, context));
                    ExecuteMigrations(migrations, upgrade);

                    // note: event is NOT dispatched but raised immediately
                    // so the transaction has not completed yet and context.Database is available
                    Migrated.RaiseEvent(new MigrationEventArgs(migrationTypes, context, _currentVersion, _targetVersion, _productName, false), this);

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                // the scope did not complete, and the transaction rolled back - BUT MySql does not support transactional
                // schema changes (http://dev.mysql.com/doc/refman/5.0/en/implicit-commit.html) - so the database is now
                // in an unknown state - raise a warning
                if (_scopeProvider.SqlContext.DatabaseType.IsMySql())
                {
                    throw new DataLossException(
                        "An error occurred running a schema migration but the changes could not be rolled back. Error: " + ex.Message
                        + ". In some cases, it may be required that the database be restored to it's original state before running this upgrade process again.",
                        ex);
                }

                // else just rethrow
                throw;
            }

            return true;
        }

        // gets all migration types
        private IEnumerable<Type> GetMigrationTypes()
        {
            return _migrationTypes ?? _migrationBuilder.MigrationTypes;
        }

        // filters and order migrations for upgrade, based upon _currentVersion and _targetVersion
        // internal for tests
        internal IEnumerable<Type> OrderedUpgradeMigrations(IEnumerable<Type> migrations)
        {
            // get the version instance to compare with the migrations as a normal c# Version object with only 3 parts
            var targetVersionToCompare = _targetVersion.GetVersion(3);
            var currentVersionToCompare = _currentVersion.GetVersion(3);

            return migrations
                .Select(x => new { migrationType = x, migrationAttributes = x.GetCustomAttributes<MigrationAttribute>(false) })
                .SelectMany(x => x.migrationAttributes, (xx, migrationAttribute) => new { xx.migrationType, migrationAttribute })
                .Where(x =>
                    x.migrationAttribute != null
                    && x.migrationAttribute.TargetVersion > currentVersionToCompare
                    && x.migrationAttribute.TargetVersion <= targetVersionToCompare
                    && x.migrationAttribute.ProductName == _productName
                    && (x.migrationAttribute.MinimumCurrentVersion == null || currentVersionToCompare >= x.migrationAttribute.MinimumCurrentVersion))
                .OrderBy(x => x.migrationAttribute.TargetVersion)
                .ThenBy(x => x.migrationAttribute.SortOrder)
                .Select(x => x.migrationType).Distinct();
        }

        // filters and order migrations for downgrade, based upon _currentVersion and _targetVersion
        // internal for tests
        internal IEnumerable<Type> OrderedDowngradeMigrations(IEnumerable<Type> migrations)
        {
            // get the version instance to compare with the migrations as a normal c# Version object with only 3 parts
            var targetVersionToCompare = _targetVersion.GetVersion(3);
            var currentVersionToCompare = _currentVersion.GetVersion(3);

            return migrations
                .Select(x => new { migrationType = x, migrationAttributes = x.GetCustomAttributes<MigrationAttribute>(false) })
                .SelectMany(x => x.migrationAttributes, (xx, migrationAttribute) => new { xx.migrationType, migrationAttribute })
                .Where(x =>
                    x.migrationAttribute != null
                    && x.migrationAttribute.TargetVersion > currentVersionToCompare
                    && x.migrationAttribute.TargetVersion <= targetVersionToCompare
                    && x.migrationAttribute.ProductName == _productName
                    && (x.migrationAttribute.MinimumCurrentVersion == null || currentVersionToCompare >= x.migrationAttribute.MinimumCurrentVersion))
                .OrderBy(x => x.migrationAttribute.TargetVersion)
                .ThenByDescending(x => x.migrationAttribute.SortOrder)
                .Select(x => x.migrationType).Distinct();
        }

        // executes migrations
        // within a transaction
        internal void ExecuteMigrations(IEnumerable<IMigration> migrations, bool upgrade = true)
        {
            foreach (var migration in migrations)
            {
                if (upgrade) migration.Up();
                else migration.Down();
            }

            var exists = _migrationEntryService.FindEntry(_productName, _targetVersion); // fixme refactor
            if (exists == null)
                _migrationEntryService.CreateEntry(_productName, _targetVersion);
        }

        /// <summary>
        /// Occurs before migrations run.
        /// </summary>
        public static event TypedEventHandler<MigrationRunner, MigrationEventArgs> Migrating;

        /// <summary>
        /// Occurs after migrations run.
        /// </summary>
        public static event TypedEventHandler<MigrationRunner, MigrationEventArgs> Migrated;
    }
}
