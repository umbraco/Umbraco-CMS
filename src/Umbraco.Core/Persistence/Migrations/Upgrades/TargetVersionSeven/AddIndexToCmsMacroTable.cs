using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// Creates a unique index on the macro alias so we cannot have duplicates by alias
    /// </summary>
    [Migration("7.0.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class AddIndexToCmsMacroTable : MigrationBase
    {
        public override void Up()
        {
            Create.Index("IX_cmsMacro_Alias").OnTable("cmsMacro").OnColumn("macroAlias").Unique();            
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}