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

            // kill unused parentId column
            Delete.Column("ParentId").FromTable(Constants.DatabaseSchema.Tables.Tag).Do();
        }
    }
}
