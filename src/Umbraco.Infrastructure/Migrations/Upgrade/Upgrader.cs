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
    /// Executes.
    /// </summary>
    /// <param name="scopeProvider">A scope provider.</param>
    /// <param name="keyValueService">A key-value service.</param>
    [Obsolete("Please use the Execute method that accepts an Umbraco.Cms.Core.Scoping.ICoreScopeProvider instead.")]
    public ExecutedMigrationPlan Execute(IMigrationPlanExecutor migrationPlanExecutor, IScopeProvider scopeProvider,
        IKeyValueService keyValueService)
        => Execute(migrationPlanExecutor, (ICoreScopeProvider)scopeProvider, keyValueService);

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
        if (scopeProvider == null)
        {
            throw new ArgumentNullException(nameof(scopeProvider));
        }

        if (keyValueService == null)
        {
            throw new ArgumentNullException(nameof(keyValueService));
        }

        // TODO: Ensure that state is always returned and saved, regardless of errors.

        var initialState = GetInitialState(scopeProvider, keyValueService);

        // execute plan
        var state = migrationPlanExecutor.Execute(Plan, initialState);
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new InvalidOperationException("Plan execution returned an invalid null or empty state.");
        }

        SetState(state, scopeProvider, keyValueService);

        // TODO: Publish notification.

        return new ExecutedMigrationPlan(Plan, initialState, state);
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
