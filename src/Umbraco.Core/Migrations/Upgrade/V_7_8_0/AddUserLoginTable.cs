using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_8_0
{
    internal class AddUserLoginTable : MigrationBase
    {
        public AddUserLoginTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains(Constants.DatabaseSchema.Tables.UserLogin) == false)
            {
                Create.Table<UserLoginDto>().Do();
            }
        }
    }
}
