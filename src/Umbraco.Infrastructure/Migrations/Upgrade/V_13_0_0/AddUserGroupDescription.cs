using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddUserGroupDescription : MigrationBase
{
    public AddUserGroupDescription(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!ColumnExists(Core.Constants.DatabaseSchema.Tables.UserGroup, "description"))
        {
            // Add the new column
            AddColumn<UserGroupDto>("description");

        }
    }
}
