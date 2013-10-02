using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// Creats a unique index across two columns so we cannot have duplicate property aliases for one macro
    /// </summary>
    [Migration("7.0.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class AddIndexToCmsMacroPropertyTable : MigrationBase
    {
        public override void Up()
        {
            Create.Index("IX_cmsMacroProperty_Alias").OnTable("cmsMacroProperty")
                  .OnColumn("macro")
                  .Ascending()
                  .OnColumn("macroPropertyAlias")
                  .Unique();
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}