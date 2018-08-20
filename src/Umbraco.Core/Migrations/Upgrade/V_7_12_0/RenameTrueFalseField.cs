using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    public class RenameTrueFalseField : MigrationBase
    {
        public RenameTrueFalseField(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            //rename the existing true/false field
            Update.Table(NodeDto.TableName).Set(new { text = "Checkbox" }).Where(new { id = Constants.DataTypes.Boolean }).Do();
        }
    }
}
