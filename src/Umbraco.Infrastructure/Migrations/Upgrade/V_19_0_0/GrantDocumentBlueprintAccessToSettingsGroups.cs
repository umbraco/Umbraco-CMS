using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Grants document blueprint access to the user groups that could reach blueprints before they moved out
///     of the Settings section.
/// </summary>
/// <remarks>
///     Runs in the main plan rather than alongside the column that
///     <see cref="AddDocumentBlueprintStartNodeToUserGroup"/> adds, so that who can do what only changes once
///     an upgrade has been approved.
/// </remarks>
public class GrantDocumentBlueprintAccessToSettingsGroups : AsyncMigrationBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GrantDocumentBlueprintAccessToSettingsGroups"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public GrantDocumentBlueprintAccessToSettingsGroups(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override async Task MigrateAsync()
    {
        // Blueprints were gated by the Settings section until they moved to Library, so the groups that
        // held Settings are the ones that already managed them. Every other group is left alone.
        Sql<ISqlContext> groupsWithSettingsAccess = GroupsWithAccessTo(Constants.Applications.Settings);

        await GrantLibrarySectionAsync(groupsWithSettingsAccess);
        await GrantBlueprintStartNodeAsync(groupsWithSettingsAccess);
    }

    private Sql<ISqlContext> GroupsWithAccessTo(string appAlias) => Database.SqlContext.Sql()
        .Select<UserGroup2AppDto>(x => x.UserGroupId)
        .From<UserGroup2AppDto>()
        .Where<UserGroup2AppDto>(x => x.AppAlias == appAlias);

    private async Task GrantLibrarySectionAsync(Sql<ISqlContext> userGroups)
    {
        // The blueprint tree is reached through the Library section.
        Sql<ISqlContext> groupsWithoutLibraryAccess = Database.SqlContext.Sql()
            .Select<UserGroupDto>(x => x.Id)
            .From<UserGroupDto>()
            .WhereIn<UserGroupDto>(x => x.Id, userGroups)
            .WhereNotIn<UserGroupDto>(x => x.Id, GroupsWithAccessTo(Constants.Applications.Library));

        List<int> userGroupIds = await Database.FetchAsync<int>(groupsWithoutLibraryAccess);

        await Database.InsertBulkAsync(userGroupIds.Select(userGroupId => new UserGroup2AppDto
        {
            UserGroupId = userGroupId,
            AppAlias = Constants.Applications.Library,
        }));
    }

    private async Task GrantBlueprintStartNodeAsync(Sql<ISqlContext> userGroups)
    {
        // Groups that already have a start node keep it, so re-running cannot widen a narrower scope.
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Update<UserGroupDto>(u => u.Set(x => x.StartDocumentBlueprintId, Constants.System.Root))
            .WhereIn<UserGroupDto>(x => x.Id, userGroups)
            .Where<UserGroupDto>(x => x.StartDocumentBlueprintId == null);

        await Database.ExecuteAsync(sql);
    }
}
