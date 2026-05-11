using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
using Umbraco.Cms.Core.Services.OperationStatus;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageMigrationRunner"/> class, responsible for running package migrations during installation or upgrade.
    /// </summary>
    /// <param name="profilingLogger">The logger used for profiling and diagnostic output during migration execution.</param>
    /// <param name="scopeProvider">Provides database transaction scopes for migration operations.</param>
    /// <param name="pendingPackageMigrations">Tracks and manages migrations that are pending execution for installed packages.</param>
    /// <param name="packageMigrationPlans">A collection of migration plans that define the steps required for package upgrades or installations.</param>
    /// <param name="migrationPlanExecutor">Executes the defined migration plans.</param>
    /// <param name="keyValueService">Service for storing and retrieving key-value pairs, often used for migration state tracking.</param>
    /// <param name="eventAggregator">Publishes and subscribes to events related to migration progress and completion.</param>
    /// <param name="logger">A typed logger instance for logging migration runner activity.</param>
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

    /// <summary>
    /// Synchronously runs any pending migrations for the specified package.
    /// </summary>
    /// <param name="packageName">The name of the package for which to run pending migrations.</param>
    /// <returns>An enumerable containing details of the executed migration plans, if any were run; otherwise, an empty enumerable.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="RunPackageMigrationsIfPendingAsync(string)"/> instead.
    /// </remarks>
    [Obsolete("Please use RunPackageMigrationsIfPendingAsync instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ExecutedMigrationPlan> RunPackageMigrationsIfPending(string packageName)
        => RunPackageMigrationsIfPendingAsync(packageName).GetAwaiter().GetResult();

    /// <summary>
    ///     Runs all migration plans for a package name if any are pending.
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ExecutedMigrationPlan>> RunPackageMigrationsIfPendingAsync(string packageName)
    {
        IReadOnlyDictionary<string, string?>? keyValues = _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);
        IReadOnlyList<string> pendingMigrations = _pendingPackageMigrations.GetPendingPackageMigrations(keyValues);

        IEnumerable<string> packagePlans = _packageMigrationPlans.Values
            .Where(x => x.PackageName.InvariantEquals(packageName))
            .Where(x => pendingMigrations.Contains(x.Name))
            .Select(x => x.Name);

        return await RunPackagePlansAsync(packagePlans).ConfigureAwait(false);
    }

    /// <summary>
    ///     Runs any pending migrations for the specified package and checks if all executed package migrations succeeded.
    /// </summary>
    /// <param name="packageName">The name of the package to run migrations for.</param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="Attempt{TResult, TStatus}"/> where <c>TResult</c> is a <see cref="bool"/> indicating success or failure, and <c>TStatus</c> is a <see cref="PackageMigrationOperationStatus"/> describing the result of the migration operation.
    /// </returns>
    public async Task<Attempt<bool, PackageMigrationOperationStatus>> RunPendingPackageMigrations(string packageName)
    {
        // Check if there are any migrations (note that the key for _packageMigrationPlans is the migration plan name, not the package name).
        if (_packageMigrationPlans.Values
            .Any(x => x.PackageName.InvariantEquals(packageName)) is false)
        {
            return Attempt.FailWithStatus(PackageMigrationOperationStatus.NotFound, false);
        }

        // Run the migrations
        IEnumerable<ExecutedMigrationPlan> executedMigrationPlans = await RunPackageMigrationsIfPendingAsync(packageName).ConfigureAwait(false);

        if (executedMigrationPlans.Any(plan => plan.Successful == false))
        {
            return Attempt.FailWithStatus(PackageMigrationOperationStatus.CancelledByFailedMigration, false);
        }

        return Attempt.SucceedWithStatus(PackageMigrationOperationStatus.Success, true);
    }

    /// <summary>
    /// Runs the specified package migration plans synchronously.
    /// </summary>
    /// <param name="plansToRun">A collection of package migration plan names to run.</param>
    /// <returns>An enumerable of executed migration plans.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="RunPackageMigrationsIfPendingAsync"/> instead.
    /// </remarks>
    [Obsolete("Please use RunPackageMigrationsIfPendingAsync instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ExecutedMigrationPlan> RunPackagePlans(IEnumerable<string> plansToRun)
        => RunPackagePlansAsync(plansToRun).GetAwaiter().GetResult();

    /// <summary>
    ///     Runs the all specified package migration plans and publishes a <see cref="MigrationPlansExecutedNotification" />
    ///     if all are successful.
    /// </summary>
    /// <param name="plansToRun"></param>
    /// <returns></returns>
    /// <exception cref="Exception">If any plan fails it will throw an exception.</exception>
    public async Task<IEnumerable<ExecutedMigrationPlan>> RunPackagePlansAsync(IEnumerable<string> plansToRun)
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
                       "Starting package migration for " + migrationName,
                       "Package migration completed for " + migrationName))
            {
                Upgrader upgrader = new(plan);

                // This may throw, if so the transaction will be rolled back
                results.Add(await upgrader.ExecuteAsync(_migrationPlanExecutor, _scopeProvider, _keyValueService).ConfigureAwait(false));
            }
        }

        var executedPlansNotification = new MigrationPlansExecutedNotification(results);
        _eventAggregator.Publish(executedPlansNotification);

        return results;
    }
}
