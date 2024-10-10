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
    ///     Initializes a new instance of the <see ref="Upgrader" /> class.
    /// </summary>
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
    ///     Executes.
    /// </summary>
    /// <param name="migrationPlanExecutor"></param>
    /// <param name="scopeProvider">A scope provider.</param>
    /// <param name="keyValueService">A key-value service.</param>
    public ExecutedMigrationPlan Execute(
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

        ExecutedMigrationPlan result = migrationPlanExecutor.ExecutePlan(Plan, initialState);

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

    private void SetState(string state, ICoreScopeProvider scopeProvider, IKeyValueService keyValueService)
    {
        using ICoreScope scope = scopeProvider.CreateCoreScope();
        keyValueService.SetValue(StateValueKey, state);
        scope.Complete();
    }
}
