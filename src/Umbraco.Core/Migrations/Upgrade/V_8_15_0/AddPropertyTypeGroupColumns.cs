using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_15_0
{
    public class AddPropertyTypeGroupColumns : MigrationBase
    {
        public AddPropertyTypeGroupColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
          
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "parentId");
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "icon");
        }
    }
}
