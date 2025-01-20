namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

public class LocalLinkMigrationTracker
{
    public bool HasFixedMigrationRun { get; private set; }

    public void MarkFixedMigrationRan() => HasFixedMigrationRun = true;
}
