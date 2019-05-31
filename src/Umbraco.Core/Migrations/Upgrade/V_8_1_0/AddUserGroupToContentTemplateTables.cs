using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_0
{
    public class AddUserGroupToContentTemplateTables : MigrationBase
    {
        public AddUserGroupToContentTemplateTables(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToList();

            if (tables.InvariantContains("umbracoUserGroup2ContentTemplate") == false)
            {
                Create.Table<UserGroup2ContentTemplateDto>().Do();
            }
        }
    }
}
