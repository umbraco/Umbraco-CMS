using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class TagsMigration : MigrationBase
    {
        public TagsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // alter columns => non-null
            AlterColumn<TagDto>(Constants.DatabaseSchema.Tables.Tag, "group");
            AlterColumn<TagDto>(Constants.DatabaseSchema.Tables.Tag, "tag");

            //AddColumn<TagDto>(Constants.DatabaseSchema.Tables.Tag, "key");

            // kill unused parentId column
            var allConstraints = Context.SqlContext.SqlSyntax.GetConstraintsPerTable(Database);
            var tableConstraints = allConstraints.Where(x => x.Item1.InvariantEquals("cmstags"));
            var exists = tableConstraints.Any(x => x.Item2 == "FK_cmsTags_cmsTags");
            if (exists)
                Delete.ForeignKey("FK_cmsTags_cmsTags").OnTable(Constants.DatabaseSchema.Tables.Tag).Do();
            Delete.Column("ParentId").FromTable(Constants.DatabaseSchema.Tables.Tag).Do();
        }
    }
}
