using System;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class AddMacroUniqueIdColumn : MigrationBase
    {
        public AddMacroUniqueIdColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsMacro") && x.ColumnName.InvariantEquals("uniqueId")) == false)
            {
                Create.Column("uniqueId").OnTable("cmsMacro").AsGuid().Nullable().Do();
                UpdateMacroGuids();
                Alter.Table("cmsMacro").AlterColumn("uniqueId").AsGuid().NotNullable().Do();
                Create.Index("IX_cmsMacro_UniqueId").OnTable("cmsMacro").OnColumn("uniqueId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique()
                    .Do();

            }

            if (columns.Any(x => x.TableName.InvariantEquals("cmsMacroProperty") && x.ColumnName.InvariantEquals("uniquePropertyId")) == false)
            {
                Create.Column("uniquePropertyId").OnTable("cmsMacroProperty").AsGuid().Nullable().Do();
                UpdateMacroPropertyGuids();
                Alter.Table("cmsMacroProperty").AlterColumn("uniquePropertyId").AsGuid().NotNullable().Do();
                Create.Index("IX_cmsMacroProperty_UniquePropertyId").OnTable("cmsMacroProperty").OnColumn("uniquePropertyId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique()
                    .Do();
            }
        }

        private void UpdateMacroGuids()
        {
            var database = Database;

            var updates = database.Query<dynamic>("SELECT id, macroAlias FROM cmsMacro")
                .Select(macro => Tuple.Create((int) macro.id, ("macro____" + (string) macro.macroAlias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE cmsMacro set uniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });
        }

        private void UpdateMacroPropertyGuids()
        {
            var database = Database;

            var updates = database.Query<dynamic>(@"SELECT cmsMacroProperty.id id, macroPropertyAlias propertyAlias, cmsMacro.macroAlias macroAlias
FROM cmsMacroProperty
JOIN cmsMacro ON cmsMacroProperty.macro=cmsMacro.id")
                .Select(prop => Tuple.Create((int) prop.id, ("macro____" + (string) prop.macroAlias + "____" + (string) prop.propertyAlias).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE cmsMacroProperty set uniquePropertyId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });
        }
    }
}
