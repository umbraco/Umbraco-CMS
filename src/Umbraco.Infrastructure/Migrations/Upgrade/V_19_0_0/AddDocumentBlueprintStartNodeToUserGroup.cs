using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>startDocumentBlueprintId</c> column to the <c>umbracoUserGroup</c> table.
/// </summary>
/// <remarks>
///     Runs as a premigration: signing in loads the user's groups, and every user group query selects this
///     column, so it has to exist before the main plan runs. Only the column is added here. Deciding which
///     groups keep blueprint access is left to <see cref="GrantDocumentBlueprintAccessToSettingsGroups"/> in
///     the main plan, so nothing about who can do what changes before an upgrade is approved.
/// </remarks>
public class AddDocumentBlueprintStartNodeToUserGroup : AsyncMigrationBase
{
    private const string ColumnName = "startDocumentBlueprintId";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddDocumentBlueprintStartNodeToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddDocumentBlueprintStartNodeToUserGroup(IMigrationContext context)
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
