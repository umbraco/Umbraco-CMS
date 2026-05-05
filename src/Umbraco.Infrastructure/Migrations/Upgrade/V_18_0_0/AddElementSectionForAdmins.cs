using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Migration that adds the Element content type infrastructure to the database,
/// including tables, recycle bin, user group permissions, and relation types.
/// </summary>
public class AddElementSectionForAdmins : AsyncMigrationBase
{
    private readonly IUserGroupService _userGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddElementSectionForAdmins"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="userGroupService">The user group service used to manage user groups.</param>
    public AddElementSectionForAdmins(IMigrationContext context, IUserGroupService userGroupService)
        : base(context)
        => _userGroupService = userGroupService;

    /// <inheritdoc />
    protected override async Task MigrateAsync()
    {
        IUserGroup? adminGroup = await _userGroupService.GetAsync(Constants.Security.AdminGroupKey);
        if (adminGroup is null || adminGroup.AllowedSections.Contains(Constants.Applications.Library))
        {
            return;
        }

        adminGroup.AddAllowedSection(Constants.Applications.Library);
        await _userGroupService.UpdateAsync(adminGroup, Constants.Security.SuperUserKey);
    }
}
