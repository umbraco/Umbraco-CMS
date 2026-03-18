using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Represents a migration that adds the <c>UserClientId</c> column to the relevant database table during the upgrade to version 15.0.0.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class AddUserClientId : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddUserClientId"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration.</param>
    public AddUserClientId(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.User2ClientId))
        {
            return;
        }

        Create.Table<User2ClientIdDto>().Do();
    }
}
