using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Adds the <see cref="UserGroup2PermissionDto"/> table to the database.
/// </summary>
public class AddUserGroup2PermisionTable : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddUserGroup2PermisionTable"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddUserGroup2PermisionTable(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Adds the <see cref="UserGroup2PermissionDto"/> table to the database.
    /// </summary>
    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup2Permission))
        {
            return;
        }

        Create.Table<UserGroup2PermissionDto>().Do();
    }
}
