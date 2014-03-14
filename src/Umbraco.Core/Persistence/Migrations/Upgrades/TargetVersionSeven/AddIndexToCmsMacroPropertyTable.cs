using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

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
            var dbIndexes = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName == "IX_cmsMacroProperty_Alias") == false)
            {
                Create.Index("IX_cmsMacroProperty_Alias").OnTable("cmsMacroProperty")
                  .OnColumn("macro")
                  .Ascending()
                  .OnColumn("macroPropertyAlias")
                  .Unique();
            }

            
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}