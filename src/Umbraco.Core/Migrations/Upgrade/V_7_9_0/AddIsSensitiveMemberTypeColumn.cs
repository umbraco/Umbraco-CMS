using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_9_0
{
    internal class AddIsSensitiveMemberTypeColumn : MigrationBase
    {
        public AddIsSensitiveMemberTypeColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.MemberPropertyType) && x.ColumnName.InvariantEquals("isSensitive")) == false)
                AddColumn<MemberPropertyTypeDto>("isSensitive");
        }
    }
}
