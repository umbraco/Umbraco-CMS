using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// Creates a unique index on the macro alias so we cannot have duplicates by alias
    /// </summary>
    [Migration("7.0.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class AddIndexToCmsMacroTable : MigrationBase
    {
        public override void Up()
        {
            var dbIndexes = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName == "IX_cmsMacro_Alias") == false)
            {
                Create.Index("IX_cmsMacro_Alias").OnTable("cmsMacro").OnColumn("macroAlias").Unique();            
            }
            
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}