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

            // Add new columns
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "parentKey");
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "level");
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "icon");

            // Create self-referencing foreign key
            var constraints = SqlSyntax.GetConstraintsPerTable(Context.Database).ToList();
            if (!constraints.Any(x => x.Item1.InvariantEquals(PropertyTypeGroupDto.TableName) && x.Item2.InvariantEquals("FK_cmsPropertyTypeGroup_parentKey")))
            {
                Create.ForeignKey("FK_cmsPropertyTypeGroup_parentKey")
                    .FromTable(PropertyTypeGroupDto.TableName).ForeignColumn("parentKey")
                    .ToTable(PropertyTypeGroupDto.TableName).PrimaryColumn("uniqueID")
                    .Do();
            }
        }
    }
}
