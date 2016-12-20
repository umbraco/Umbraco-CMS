using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddMacroUniqueIdColumn : MigrationBase
    {
        public AddMacroUniqueIdColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger) 
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsMacro") && x.ColumnName.InvariantEquals("uniqueId")) == false)
            {
                Create.Column("uniqueId").OnTable("cmsMacro").AsGuid().Nullable();
                Execute.Code(UpdateMacroGuids);
                Alter.Column("uniqueId").OnTable("cmsMacro").AsGuid().NotNullable();
                Create.Index("IX_cmsMacroUniqueId").OnTable("cmsMacro").OnColumn("uniqueId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique();

            }

            if (columns.Any(x => x.TableName.InvariantEquals("cmsMacroProperty") && x.ColumnName.InvariantEquals("uniqueId")) == false)
            {
                Create.Column("uniqueId").OnTable("cmsMacroProperty").AsGuid().Nullable();
                Execute.Code(UpdateMacroPropertyGuids);
                Alter.Column("uniqueId").OnTable("cmsMacroProperty").AsGuid().NotNullable();
                Create.Index("IX_cmsMacroPropertyUniqueId").OnTable("cmsMacroProperty").OnColumn("uniqueId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique();
            }
        }

        private static string UpdateMacroGuids(Database database)
        {
            var updates = database.Query<dynamic>("SELECT id, macroAlias FROM cmsMacro")
                .Select(macro => Tuple.Create((int) macro.id, ((string) macro.macroAlias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE cmsMacro set uniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });

            return string.Empty;
        }

        private static string UpdateMacroPropertyGuids(Database database)
        {
            var updates = database.Query<dynamic>("SELECT id, macroPropertyAlias FROM cmsMacroProperty")
                .Select(macro => Tuple.Create((int) macro.id, ((string) macro.macroPropertyAlias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE cmsMacroProperty set uniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });

            return string.Empty;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
