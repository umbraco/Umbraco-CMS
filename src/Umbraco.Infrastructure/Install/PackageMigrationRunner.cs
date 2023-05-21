using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly ILogger<PackageMigrationRunner> _logger;
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
        IEventAggregator eventAggregator,
        ILogger<PackageMigrationRunner> logger)
    {
        _profilingLogger = profilingLogger;
        _scopeProvider = scopeProvider;
        _pendingPackageMigrations = pendingPackageMigrations;
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _packageMigrationPlans = packageMigrationPlans.ToDictionary(x => x.Name);
    }

    [Obsolete("Use constructor that takes ILogger, this will be removed in V13")]
    public PackageMigrationRunner(
        IProfilingLogger profilingLogger,
        ICoreScopeProvider scopeProvider,
        PendingPackageMigrations pendingPackageMigrations,
        PackageMigrationPlanCollection packageMigrationPlans,
        IMigrationPlanExecutor migrationPlanExecutor,
        IKeyValueService keyValueService,
        IEventAggregator eventAggregator)
        : this(
            profilingLogger,
            scopeProvider,
            pendingPackageMigrations,
            packageMigrationPlans,
            migrationPlanExecutor,
            keyValueService,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<PackageMigrationRunner>>())
    {
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
        List<ExecutedMigrationPlan> results = new();

        // We don't create an explicit scope, the Upgrader will handle that depending on the migration type.
        // We also run ALL the package migration to completion, even if one fails, my package should not fail if someone else's package does
        foreach (var migrationName in plansToRun)
        {
            if (_packageMigrationPlans.TryGetValue(migrationName, out PackageMigrationPlan? plan) is false)
            {
                // If we can't find the migration plan for a package we'll just log a message and continue.
                _logger.LogError("Package migration failed for {migrationName}, was unable to find the migration plan", migrationName);
                continue;
            }

            using (_profilingLogger.TraceDuration<PackageMigrationRunner>(
                       "Starting unattended package migration for " + migrationName,
                       "Unattended upgrade completed for " + migrationName))
            {
                Upgrader upgrader = new(plan);

                // This may throw, if so the transaction will be rolled back
                results.Add(upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService));
            }
        }

        var executedPlansNotification = new MigrationPlansExecutedNotification(results);
        _eventAggregator.Publish(executedPlansNotification);

        return results;
    }
}
