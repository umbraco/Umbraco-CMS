using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <summary>
///     Used to run a <see cref="MigrationPlan" />
/// </summary>
public class Upgrader
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Upgrader" /> class with the specified migration plan.
    /// </summary>
    /// <param name="plan">The <see cref="MigrationPlan"/> to use for the upgrade process.</param>
    public Upgrader(MigrationPlan plan) => Plan = plan;

    /// <summary>
    ///     Gets the name of the migration plan.
    /// </summary>
    public string Name => Plan.Name;

    /// <summary>
    ///     Gets the migration plan.
    /// </summary>
    public MigrationPlan Plan { get; }

    /// <summary>
    ///     Gets the key for the state value.
    /// </summary>
    public virtual string StateValueKey => Constants.Conventions.Migrations.KeyValuePrefix + Name;

    /// <summary>
    /// Executes the migration plan synchronously using the provided migration plan executor, scope provider, and key value service.
    /// </summary>
    /// <param name="migrationPlanExecutor">The executor responsible for running the migration plan.</param>
    /// <param name="scopeProvider">The scope provider used to manage database scopes during migration.</param>
    /// <param name="keyValueService">The key value service used for storing migration state.</param>
    /// <returns>The result of the executed migration plan.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ExecuteAsync"/> instead.
    /// </remarks>
    [Obsolete("Use ExecuteAsync instead. Scheduled for removal in Umbraco 18.")]
    public ExecutedMigrationPlan Execute(
        IMigrationPlanExecutor migrationPlanExecutor,
        ICoreScopeProvider scopeProvider,
        IKeyValueService keyValueService)
        => ExecuteAsync(migrationPlanExecutor, scopeProvider, keyValueService).GetAwaiter().GetResult();

    /// <summary>
    ///     Executes the migration plan asynchronously using the specified executor, scope provider, and key-value service.
    /// </summary>
    /// <param name="migrationPlanExecutor">The executor responsible for running the migration plan.</param>
    /// <param name="scopeProvider">The provider used to manage database transaction scopes.</param>
    /// <param name="keyValueService">The service used to persist and retrieve key-value pairs for migration state tracking.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the executed migration plan.</returns>
    public async Task<ExecutedMigrationPlan> ExecuteAsync(
        IMigrationPlanExecutor migrationPlanExecutor,
        ICoreScopeProvider scopeProvider,
        IKeyValueService keyValueService)
    {
        if (scopeProvider is null)
        {
            throw new ArgumentNullException(nameof(scopeProvider));
        }

        if (keyValueService is null)
        {
            throw new ArgumentNullException(nameof(keyValueService));
        }

        string initialState = GetInitialState(scopeProvider, keyValueService);

        ExecutedMigrationPlan result = await migrationPlanExecutor.ExecutePlanAsync(Plan, initialState).ConfigureAwait(false);

        // This should never happen, if the final state comes back as null or equal to the initial state
        // it means that no transitions was successful, which means it cannot be a successful migration
        if (result.Successful && string.IsNullOrWhiteSpace(result.FinalState))
        {
            throw new InvalidOperationException("Plan execution returned an invalid null or empty state.");
        }

        // Otherwise it just means that our migration failed on the first step, which is fine,
        // or there were no pending transitions so nothing changed.
        // We will skip saving the state since we it's still the same
        if (result.FinalState == result.InitialState)
        {
            return result;
        }

        return result;
    }

    private string GetInitialState(ICoreScopeProvider scopeProvider, IKeyValueService keyValueService)
    {
        using ICoreScope scope = scopeProvider.CreateCoreScope();
        var currentState = keyValueService.GetValue(StateValueKey);
        scope.Complete();

        if (currentState is null || Plan.IgnoreCurrentState)
        {
            return Plan.InitialState;
        }

        return currentState;
    }
}
