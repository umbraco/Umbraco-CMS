namespace Umbraco.Cms.Infrastructure.Migrations;

public class ExecutedEFCoreMigrationPlan
{
    public ExecutedEFCoreMigrationPlan(EFCoreMigrationPlan plan, string initialState, string finalState)
    {
        Plan = plan;
        InitialState = initialState;
        FinalState = finalState;
    }

    public EFCoreMigrationPlan Plan { get; }

    public string InitialState { get; }

    public string FinalState { get; }
}
