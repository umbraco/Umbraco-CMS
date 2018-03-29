using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_8_0
{
    internal class AddTourDataUserColumn : MigrationBase
    {
        public AddTourDataUserColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.User) && x.ColumnName.InvariantEquals("tourData")) == false)
                AddColumn<UserDto>("tourData");
        }
    }
}
