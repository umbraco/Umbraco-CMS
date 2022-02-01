using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_10_0
{

    public class AddPropertyTypeLabelOnTopColumn : MigrationBase
    {
        public AddPropertyTypeLabelOnTopColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<PropertyTypeDto>(columns, "labelOnTop");
        }
    }
}
