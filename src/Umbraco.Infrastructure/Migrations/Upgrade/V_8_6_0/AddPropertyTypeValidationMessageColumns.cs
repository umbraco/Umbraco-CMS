using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{

    public class AddPropertyTypeValidationMessageColumns : MigrationBase
    {
        public AddPropertyTypeValidationMessageColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<PropertyTypeDto>(columns, "mandatoryMessage");
            AddColumnIfNotExists<PropertyTypeDto>(columns, "validationRegExpMessage");
        }
    }
}
