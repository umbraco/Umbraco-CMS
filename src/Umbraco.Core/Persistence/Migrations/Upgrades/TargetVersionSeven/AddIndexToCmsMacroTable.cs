using System;
using System.CodeDom;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
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
        private readonly bool _forTesting;

        internal AddIndexToCmsMacroTable(bool forTesting, ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
            _forTesting = forTesting;
        }

        public AddIndexToCmsMacroTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var dbIndexes = _forTesting ? new DbIndexDefinition[] { } : SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsMacro_Alias")) == false)
            {
                //in order to create this index, we need to ensure that there are no duplicates. This could have happened with very old/corrupt umbraco versions.
                // So we'll remove any duplicates based on alias and only keep the one with the smallest id since I'm pretty sure we'd always choose the 'first' one
                // when running a query.

                if (_forTesting == false)
                {
                    //NOTE: Using full SQL statement here in case the DTO has changed between versions
                    var macros = Context.Database.Fetch<MacroDto>("SELECT * FROM cmsMacro")
                        .GroupBy(x => x.Alias)
                        .Where(x => x.Count() > 1);

                    foreach (var m in macros)
                    {
                        //get the min id (to keep)
                        var minId = m.Min(x => x.Id);
                        //delete all the others
                        foreach (var macroDto in m.Where(x => x.Id != minId))
                        {
                            Delete.FromTable("cmsMacro").Row(new { id = macroDto.Id });
                        }
                    }
                }
                

                Create.Index("IX_cmsMacro_Alias").OnTable("cmsMacro").OnColumn("macroAlias").Unique();            
            }
            
        }

        public override void Down()
        {
            Delete.Index("IX_cmsMacro_Alias").OnTable("cmsMacro");
        }
    }
}