using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// Creats a unique index across two columns so we cannot have duplicate property aliases for one macro
    /// </summary>
    [Migration("7.0.0", 5, Constants.System.UmbracoMigrationName)]
    public class AddIndexToCmsMacroPropertyTable : MigrationBase
    {
        private readonly bool _skipIndexCheck;

        internal AddIndexToCmsMacroPropertyTable(bool skipIndexCheck, ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
            _skipIndexCheck = skipIndexCheck;
        }

        public AddIndexToCmsMacroPropertyTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var dbIndexes = _skipIndexCheck ? new DbIndexDefinition[]{} : SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition(x)).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsMacroProperty_Alias")) == false)
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
            Delete.Index("IX_cmsMacroProperty_Alias").OnTable("cmsMacroProperty");
        }
    }
}
