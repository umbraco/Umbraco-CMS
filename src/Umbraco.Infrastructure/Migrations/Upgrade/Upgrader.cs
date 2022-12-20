using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;

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
    /// <param name="aggregator"></param>
    public ExecutedMigrationPlan Execute(
        IMigrationPlanExecutor migrationPlanExecutor,
        ICoreScopeProvider scopeProvider,
        IKeyValueService keyValueService)
    {
        if (scopeProvider == null)
        {
            throw new ArgumentNullException(nameof(scopeProvider));
        }

        if (keyValueService == null)
        {
            throw new ArgumentNullException(nameof(keyValueService));
        }

        var initialState = GetInitialState(scopeProvider, keyValueService);

        // execute plan
        ExecutedMigrationPlan result = migrationPlanExecutor.Execute(Plan, initialState);

        // This should never happen, but if it does, we can't save it in the database.
        if (string.IsNullOrWhiteSpace(result.FinalState))
        {
            throw new InvalidOperationException("Plan execution returned an invalid null or empty state.");
        }

        // We always save the final state of the migration plan, this is because a partial success is possible
        // So we still want to save the place we got to in the database-
        SetState(result.FinalState, scopeProvider, keyValueService);

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
