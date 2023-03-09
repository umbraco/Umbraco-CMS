namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <inheritdoc />
public class UmbracoEFCorePlan : EFCoreMigrationPlan
{
    public UmbracoEFCorePlan(string name) : base(name)
    {
        InitialState = string.Empty;
    }

    protected void DefinePlan()
    {
        From(InitialState);

        To<V_12_0_0.AddEfCoreHistoryTable>("{F68AED62-1617-493C-9029-43DAC82A3DFB}");
    }
}
