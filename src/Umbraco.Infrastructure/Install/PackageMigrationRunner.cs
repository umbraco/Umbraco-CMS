using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
///     Runs the package migration plans
/// </summary>
public class PackageMigrationRunner
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IKeyValueService _keyValueService;
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly Dictionary<string, PackageMigrationPlan> _packageMigrationPlans;
    private readonly PendingPackageMigrations _pendingPackageMigrations;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ICoreScopeProvider _scopeProvider;

    public PackageMigrationRunner(
        IProfilingLogger profilingLogger,
        ICoreScopeProvider scopeProvider,
        PendingPackageMigrations pendingPackageMigrations,
        PackageMigrationPlanCollection packageMigrationPlans,
        IMigrationPlanExecutor migrationPlanExecutor,
        IKeyValueService keyValueService,
        IEventAggregator eventAggregator)
    {
        _profilingLogger = profilingLogger;
        _scopeProvider = scopeProvider;
        _pendingPackageMigrations = pendingPackageMigrations;
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _eventAggregator = eventAggregator;
        _packageMigrationPlans = packageMigrationPlans.ToDictionary(x => x.Name);
    }

    /// <summary>
    ///     Runs all migration plans for a package name if any are pending.
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public IEnumerable<ExecutedMigrationPlan> RunPackageMigrationsIfPending(string packageName)
    {
        IReadOnlyDictionary<string, string?>? keyValues =
            _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);
        IReadOnlyList<string> pendingMigrations = _pendingPackageMigrations.GetPendingPackageMigrations(keyValues);

        IEnumerable<string> packagePlans = _packageMigrationPlans.Values
            .Where(x => x.PackageName.InvariantEquals(packageName))
            .Where(x => pendingMigrations.Contains(x.Name))
            .Select(x => x.Name);

        return RunPackagePlans(packagePlans);
    }

    /// <summary>
    ///     Runs the all specified package migration plans and publishes a <see cref="MigrationPlansExecutedNotification" />
    ///     if all are successful.
    /// </summary>
    /// <param name="plansToRun"></param>
    /// <returns></returns>
    /// <exception cref="Exception">If any plan fails it will throw an exception.</exception>
    public IEnumerable<ExecutedMigrationPlan> RunPackagePlans(IEnumerable<string> plansToRun)
    {
        var results = new List<ExecutedMigrationPlan>();

        // Create an explicit scope around all package migrations so they are
        // all executed in a single transaction. If one package migration fails,
        // none of them will be committed. This is intended behavior so we can
        // ensure when we publish the success notification that is is done when they all succeed.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            foreach (var migrationName in plansToRun)
            {
                if (!_packageMigrationPlans.TryGetValue(migrationName, out PackageMigrationPlan? plan))
                {
                    throw new InvalidOperationException("Cannot find package migration plan " + migrationName);
                }

                using (_profilingLogger.TraceDuration<PackageMigrationRunner>(
                           "Starting unattended package migration for " + migrationName,
                           "Unattended upgrade completed for " + migrationName))
                {
                    var upgrader = new Upgrader(plan);

                    // This may throw, if so the transaction will be rolled back
                    results.Add(upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService));
                }
            }
        }

        var executedPlansNotification = new MigrationPlansExecutedNotification(results);
        _eventAggregator.Publish(executedPlansNotification);

        return results;
    }
}
