using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RemoveDefaultPermissionUserColumn : MigrationBase
    {
        public override void Up()
        {
            Delete.Column("userDefaultPermissions").FromTable("umbracoUser");
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}