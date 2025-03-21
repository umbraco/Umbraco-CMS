using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class UserServiceCrudTests
{
    [Test]
    [TestCase("test@email.com", "test@email.com", true, true)]
    [TestCase("test@email.com", "notTheUserName@email.com", true, false)]
    [TestCase("NotAnEmail", "test@email.com", true, false)]
    [TestCase("test@email.com", "test@email.com", false, true)]
    [TestCase("NotAnEmail", "test@email.com", false, true)]
    [TestCase("aDifferentEmail@email.com", "test@email.com", false, true)]
    public async Task Invite_User_Name_Must_Be_Email(
        string username,
        string email,
        bool userNameIsEmailEnabled,
        bool shouldSucceed)
    {
        var securitySettings = new SecuritySettings { UsernameIsEmail = userNameIsEmailEnabled };
        var userService = CreateUserService(securitySettings);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var inviteModel = new UserInviteModel
        {
            UserName = username,
            Email = email,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        var result = await userService.InviteAsync(Constants.Security.SuperUserKey, inviteModel);

        if (shouldSucceed is false)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.UserNameIsNotEmail, result.Status);
            return;
        }

        Assert.IsTrue(result.Success);
        Assert.AreEqual(UserOperationStatus.Success, result.Status);
        var invitedUser = result.Result.InvitedUser;
        Assert.IsNotNull(invitedUser);
        Assert.AreEqual(username, invitedUser.Username);
        Assert.AreEqual(email, invitedUser.Email);
    }

    [Test]
    public async Task Cannot_Invite_User_With_Duplicate_Email()
    {
        var email = "test@test.com";
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var initialUserCreateModel = new UserCreateModel
        {
            UserName = "Test1",
            Email = email,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.IsTrue(result.Success);

        var duplicateUserInviteModel = new UserInviteModel
        {
            UserName = "Test2",
            Email = email,
            Name = "Duplicate Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        var secondResult = await userService.InviteAsync(Constants.Security.SuperUserKey, duplicateUserInviteModel);
        Assert.IsFalse(secondResult.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateEmail, secondResult.Status);
    }

    [Test]
    public async Task Cannot_Invite_User_With_Duplicate_UserName()
    {
        var userName = "UserName";
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var initialUserCreateModel = new UserCreateModel
        {
            UserName = userName,
            Email = "test@email.com",
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.IsTrue(result.Success);

        var duplicateUserInviteModelModel = new UserInviteModel
        {
            UserName = userName,
            Email = "another@email.com",
            Name = "Duplicate Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        var secondResult = await userService.InviteAsync(Constants.Security.SuperUserKey, duplicateUserInviteModelModel);
        Assert.IsFalse(secondResult.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateUserName, secondResult.Status);
    }

    [Test]
    public async Task Cannot_Invite_User_Without_User_Group()
    {
        UserInviteModel userInviteModel = new UserInviteModel
        {
            UserName = "NoUser@Group.com",
            Email = "NoUser@Group.com",
            Name = "NoUser@Group.com",
        };

        IUserService userService = CreateUserService();

        var result = await userService.InviteAsync(Constants.Security.SuperUserKey, userInviteModel);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.NoUserGroup, result.Status);
    }

    [Test]
    public async Task Performing_User_Must_Exist_When_Inviting()
    {
        IUserService userService = CreateUserService();

        var result = await userService.InviteAsync(Guid.Empty, new UserInviteModel());

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.MissingUser, result.Status);
    }

    [Test]
    public async Task Invited_Users_Has_Invited_state()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        UserInviteModel userInviteModel = new UserInviteModel
        {
            UserName = "some@email.com",
            Email = "some@email.com",
            Name = "Bob",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
        };

        IUserService userService = CreateUserService();
        var result = await userService.InviteAsync(Constants.Security.SuperUserKey, userInviteModel);
        Assert.IsTrue(result.Success);

        var invitedUser = await userService.GetAsync(result.Result.InvitedUser!.Key);
        Assert.IsNotNull(invitedUser);
        Assert.AreEqual(UserState.Invited, invitedUser.UserState);
    }
}
