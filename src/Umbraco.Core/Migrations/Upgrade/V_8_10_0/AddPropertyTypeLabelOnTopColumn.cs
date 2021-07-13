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
            AddColumn<PropertyTypeDto>("labelOnTop");
        }
    }
}
