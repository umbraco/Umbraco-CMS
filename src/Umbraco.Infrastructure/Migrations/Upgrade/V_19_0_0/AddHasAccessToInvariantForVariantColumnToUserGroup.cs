using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>hasAccessToInvariantForVariant</c> column to the <c>umbracoUserGroup</c> table.
/// </summary>
/// <remarks>
///     The existing user groups are granted the permission by <see cref="AddHasAccessToInvariantForVariantToUserGroup"/>.
/// </remarks>
public class AddHasAccessToInvariantForVariantColumnToUserGroup : AsyncMigrationBase
{
    private const string ColumnName = "hasAccessToInvariantForVariant";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddHasAccessToInvariantForVariantColumnToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddHasAccessToInvariantForVariantColumnToUserGroup(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup, ColumnName))
        {
            return Task.CompletedTask;
        }

        AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, ColumnName);
        return Task.CompletedTask;
    }
}
