using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_16_0
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
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "type");
            AddColumnIfNotExists<PropertyTypeGroupDto>(columns, "icon");

            // Create self-referencing foreign key
            var foreignKeyName = "FK_" + PropertyTypeGroupDto.TableName + "_parentKey";
            if (SqlSyntax.GetConstraintsPerTable(Context.Database).Any(x => x.Item2.InvariantEquals(foreignKeyName)) == false)
            {
                Create.ForeignKey(foreignKeyName)
                    .FromTable(PropertyTypeGroupDto.TableName).ForeignColumn("parentKey")
                    .ToTable(PropertyTypeGroupDto.TableName).PrimaryColumn("uniqueID")
                    .Do();
            }
        }
    }
}
