using System;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class AddRelationTypeUniqueIdColumn : MigrationBase
    {
        public AddRelationTypeUniqueIdColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRelationType") && x.ColumnName.InvariantEquals("typeUniqueId")) == false)
            {
                Create.Column("typeUniqueId").OnTable("umbracoRelationType").AsGuid().Nullable().Do();
                UpdateRelationTypeGuids();
                Alter.Table("umbracoRelationType").AlterColumn("typeUniqueId").AsGuid().NotNullable().Do();
                Create.Index("IX_umbracoRelationType_UniqueId").OnTable("umbracoRelationType").OnColumn("typeUniqueId")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique()
                    .Do();
            }
        }

        private void UpdateRelationTypeGuids()
        {
            var database = Database;
            var updates = database.Query<dynamic>("SELECT id, alias, name FROM umbracoRelationType")
                .Select(relationType => Tuple.Create((int) relationType.id, ("relationType____" + (string) relationType.alias + "____" + (string) relationType.name).ToGuid()))
                .ToList();

            foreach (var update in updates)
                database.Execute("UPDATE umbracoRelationType set typeUniqueId=@guid WHERE id=@id", new { guid = update.Item2, id = update.Item1 });
        }
    }
}
