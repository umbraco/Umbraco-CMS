using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

public class EFCoreUpgrader
{
    public EFCoreUpgrader(EFCoreMigrationPlan plan) => Plan = plan;

    /// <summary>
    ///     Gets the name of the migration plan.
    /// </summary>
    public string Name => Plan.Name;

    /// <summary>
    ///     Gets the key for the state value.
    /// </summary>
    public virtual string StateValueKey => Constants.Conventions.Migrations.KeyValuePrefix + Name;

    /// <summary>
    ///     Gets the migration plan.
    /// </summary>
    public EFCoreMigrationPlan Plan { get; }

    /// <summary>
    ///     Executes.
    /// </summary>
    /// <param name="keyValueService">A key-value service.</param>
    /// <param name="migrationPlanExecutor">A key-value service.</param>
    public ExecutedEFCoreMigrationPlan Execute(IEFCoreMigrationPlanExecutor migrationPlanExecutor, IKeyValueService keyValueService)
    {
        if (keyValueService == null)
        {
            throw new ArgumentNullException(nameof(keyValueService));
        }

        // read current state
        var currentState = keyValueService.GetValue(StateValueKey) ?? string.Empty;
        var forceState = false;

        // execute plan
        var state = migrationPlanExecutor.ExecutePlan(Plan, currentState).FinalState;
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new InvalidOperationException("Plan execution returned an invalid null or empty state.");
        }

        // save new state
        if (forceState)
        {
            keyValueService.SetValue(StateValueKey, state);
        }
        else if (currentState != state)
        {
            keyValueService.SetValue(StateValueKey, currentState, state);
        }

        return new ExecutedEFCoreMigrationPlan(Plan, currentState, state);
    }
}
