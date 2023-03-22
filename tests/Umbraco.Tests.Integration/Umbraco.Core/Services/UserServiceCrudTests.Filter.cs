using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class UserServiceCrudTests
{
    [Test]
    public async Task Cannot_Request_Disabled_If_Hidden()
    {
        var userService = CreateUserService(new SecuritySettings { HideDisabledUsersInBackOffice = true });
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupAlias);

        var createModel = new UserCreateModel
        {
            UserName = "editor@mail.com",
            Email = "editor@mail.com",
            Name = "Editor",
            UserGroups = new HashSet<IUserGroup> { editorGroup! }
        };

        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(createAttempt.Success);

        var disableStatus = await userService.DisableAsync(Constants.Security.SuperUserKey, createAttempt.Result.CreatedUser!.Key);
        Assert.AreEqual(UserOperationStatus.Success, disableStatus);

        var filter = new UserFilter
        {
            IncludeUserStates = new SortedSet<UserState> { UserState.Disabled }
        };

        var filterAttempt = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);
        Assert.IsTrue(filterAttempt.Success);
        Assert.AreEqual(0, filterAttempt.Result.Items.Count());
    }
}
