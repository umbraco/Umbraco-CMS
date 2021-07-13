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

            AddColumn<PropertyTypeGroupDto>(columns, "type");

            AddColumn<PropertyTypeGroupDto>(columns, "alias", out var sqls);
            var dtos = Database.Fetch<PropertyTypeGroupDto>();
            foreach (var dto in dtos)
            {
                // Generate alias from current name
                dto.Alias = dto.Text.ToSafeAlias();

                Database.Update(dto, x => new { x.Alias });
            }
            foreach (var sql in sqls) Database.Execute(sql);
        }
    }
}
