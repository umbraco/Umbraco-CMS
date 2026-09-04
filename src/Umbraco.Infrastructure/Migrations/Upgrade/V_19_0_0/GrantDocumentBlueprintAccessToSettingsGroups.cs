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
    protected override Task MigrateAsync()
    {
        // Blueprints were gated by the Settings section until they moved to Library, so the groups that
        // held Settings are the ones that already managed them. Every other group is left without access
        // rather than inheriting it from the Library section it now shares with elements. Groups that
        // already have a start node are left alone, so re-running cannot widen a narrower scope.
        Sql<ISqlContext> groupsWithSettingsAccess = Database.SqlContext.Sql()
            .Select<UserGroup2AppDto>(x => x.UserGroupId)
            .From<UserGroup2AppDto>()
            .Where<UserGroup2AppDto>(x => x.AppAlias == Constants.Applications.Settings);

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Update<UserGroupDto>(u => u.Set(x => x.StartDocumentBlueprintId, Constants.System.Root))
            .WhereIn<UserGroupDto>(x => x.Id, Database.Fetch<int>(groupsWithSettingsAccess))
            .Where<UserGroupDto>(x => x.StartDocumentBlueprintId == null);

        Database.Execute(sql);

        return Task.CompletedTask;
    }
}
