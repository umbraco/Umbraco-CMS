using System;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 9)]
    public class EnsureAppsTreesUpdated : MigrationBase
    {
        public override void Up()
        {
            var e = new UpgradingEventArgs();

            if (Upgrading != null)
                Upgrading(this, e);
        }

        public override void Down()
        {
        }

        public static event EventHandler<UpgradingEventArgs> Upgrading;

        public class UpgradingEventArgs : EventArgs{}
    }
}