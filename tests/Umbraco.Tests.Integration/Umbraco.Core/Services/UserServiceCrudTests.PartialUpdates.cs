using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class UserServiceCrudTests
{
    [Test]
    public async Task Can_Enable_User()
    {
        var editorUserGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { editorUserGroup.Key },
        };

        var userService = CreateUserService();
        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, false);

        Assert.IsTrue(createAttempt.Success);
        var user = createAttempt.Result.CreatedUser;
        Assert.AreEqual(UserState.Disabled, user!.UserState);

        var enableStatus = await userService.EnableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { user.Key });
        Assert.AreEqual(UserOperationStatus.Success, enableStatus);

        var updatedUser = await userService.GetAsync(user.Key);
        // The user has not logged in, so after enabling the user, the user state should be inactive
        Assert.AreEqual(UserState.Inactive, updatedUser!.UserState);
    }

    [Test]
    public async Task Can_Disable_User()
    {
        var editorUserGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { editorUserGroup.Key },
        };

        var userService = CreateUserService();
        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);

        Assert.IsTrue(createAttempt.Success);
        var user = createAttempt.Result.CreatedUser;
        Assert.AreEqual(UserState.Inactive, user!.UserState);

        var disableStatus = await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { user.Key });
        Assert.AreEqual(UserOperationStatus.Success, disableStatus);
    }

    [Test]
    public async Task User_Cannot_Disable_Self()
    {
        var adminUserGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { adminUserGroup.Key },
        };

        var userService = CreateUserService();
        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);
        Assert.IsTrue(createAttempt.Success);

        var createdUser = createAttempt.Result.CreatedUser;
        var disableStatus = await userService.DisableAsync(createdUser!.Key, new HashSet<Guid>{ createdUser.Key });
        Assert.AreEqual(UserOperationStatus.CannotDisableSelf, disableStatus);
    }

    [Test]
    public async Task Cannot_Disable_Invited_User()
    {
        var editorUserGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var userInviteModel = new UserInviteModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { editorUserGroup.Key },
        };

        var userService = CreateUserService();

        var userInviteAttempt = await userService.InviteAsync(Constants.Security.SuperUserKey, userInviteModel);
        Assert.IsTrue(userInviteAttempt.Success);

        var invitedUser = userInviteAttempt.Result.InvitedUser;
        var disableStatus = await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { invitedUser!.Key });
        Assert.AreEqual(UserOperationStatus.CannotDisableInvitedUser, disableStatus);
    }

    [Test]
    public async Task Enable_Missing_User_Fails_With_Not_Found()
    {
        var editorUserGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { editorUserGroup.Key },
        };

        var userService = CreateUserService();
        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, false);

        Assert.IsTrue(createAttempt.Success);
        var user = createAttempt.Result.CreatedUser;
        Assert.AreEqual(UserState.Disabled, user!.UserState);

        var enableStatus = await userService.EnableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { user.Key, Guid.NewGuid() });
        Assert.AreEqual(UserOperationStatus.UserNotFound, enableStatus);
    }

    [Test]
    public async Task Disable_Missing_User_Fails_With_Not_Found()
    {
        var editorUserGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "test@email.com",
            Email = "test@email.com",
            Name = "Test User",
            UserGroupKeys = new HashSet<Guid> { editorUserGroup.Key },
        };

        var userService = CreateUserService();
        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);

        Assert.IsTrue(createAttempt.Success);
        var user = createAttempt.Result.CreatedUser;

        var enableStatus = await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { user.Key, Guid.NewGuid() });
        Assert.AreEqual(UserOperationStatus.UserNotFound, enableStatus);
    }
}
