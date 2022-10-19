namespace Umbraco.Cms.Infrastructure.Migrations;

public class ExecutedMigrationPlan
{
    public ExecutedMigrationPlan(MigrationPlan plan, string initialState, string finalState)
    {
        Plan = plan;
        InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
    }

    public MigrationPlan Plan { get; }

    public string InitialState { get; }

    public string FinalState { get; }
}
