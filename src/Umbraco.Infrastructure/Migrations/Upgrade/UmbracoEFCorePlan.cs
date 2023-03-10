namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <inheritdoc />
public class UmbracoEFCorePlan : EFCoreMigrationPlan
{
    public UmbracoEFCorePlan() : base("Umbraco.EFCore")
    {
        InitialState = "91A8BDA4-603E-41E6-9C45-15C787F7DD86";
        DefinePlan();
    }

    protected void DefinePlan()
    {
        From(InitialState);

        To<V_12_0_0.AddEfCoreHistoryTable>("F68AED62-1617-493C-9029-43DAC82A3DFB");
    }
}
