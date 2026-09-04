using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>startDocumentBlueprintId</c> column to the <c>umbracoUserGroup</c> table and grants
///     document blueprint access to the groups that could reach blueprints before they moved out of the
///     Settings section.
/// </summary>
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
        EnsureDocumentBlueprintStartNodeColumn();
        GrantAccessToGroupsWithSettingsAccess();

        return Task.CompletedTask;
    }

    private void EnsureDocumentBlueprintStartNodeColumn()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup, ColumnName))
        {
            return;
        }

        AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, ColumnName);
    }

    private void GrantAccessToGroupsWithSettingsAccess()
    {
        // Blueprints were gated by the Settings section until they moved to Library, so the groups that
        // held Settings are the ones that already managed them. Every other group is left without access
        // rather than inheriting it from the Library section it now shares with elements.
        Sql<ISqlContext> groupsWithSettingsAccess = Database.SqlContext.Sql()
            .Select<UserGroup2AppDto>(x => x.UserGroupId)
            .From<UserGroup2AppDto>()
            .Where<UserGroup2AppDto>(x => x.AppAlias == Constants.Applications.Settings);

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Update<UserGroupDto>(u => u.Set(x => x.StartDocumentBlueprintId, Constants.System.Root))
            .WhereIn<UserGroupDto>(x => x.Id, Database.Fetch<int>(groupsWithSettingsAccess))
            .Where<UserGroupDto>(x => x.StartDocumentBlueprintId == null);

        Database.Execute(sql);
    }
}
