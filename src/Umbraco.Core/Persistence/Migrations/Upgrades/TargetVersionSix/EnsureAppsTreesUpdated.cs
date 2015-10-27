using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 9, GlobalSettings.UmbracoMigrationName)]
    public class EnsureAppsTreesUpdated : MigrationBase
    {
        public EnsureAppsTreesUpdated(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

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