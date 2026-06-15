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
    public async Task Creating_User_Name_Must_Be_Email(
        string username,
        string email,
        bool userNameIsEmailEnabled,
        bool shouldSucceed)
    {
        var securitySettings = new SecuritySettings { UsernameIsEmail = userNameIsEmailEnabled };
        var userService = CreateUserService(securitySettings);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var creationModel = new UserCreateModel
        {
            UserName = username,
            Email = email,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, creationModel, true);

        if (shouldSucceed is false)
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(UserOperationStatus.UserNameIsNotEmail));
            return;
        }

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(UserOperationStatus.Success));
        var createdUser = result.Result.CreatedUser;
        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser.Username, Is.EqualTo(username));
        Assert.That(createdUser.Email, Is.EqualTo(email));
        Assert.That(createdUser.Kind, Is.EqualTo(UserKind.Default));
    }

    [TestCase(UserKind.Default)]
    [TestCase(UserKind.Api)]
    public async Task Can_Create_All_User_Types(UserKind kind)
    {
        var securitySettings = new SecuritySettings();
        var userService = CreateUserService(securitySettings);

        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var creationModel = new UserCreateModel
        {
            UserName = "api@local",
            Email = "api@local",
            Name = "API user",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key },
            Kind = kind
        };

        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, creationModel, true);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(UserOperationStatus.Success));
        var createdUser = result.Result.CreatedUser;
        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser.Kind, Is.EqualTo(kind));

        var user = await userService.GetAsync(createdUser.Key);
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Kind, Is.EqualTo(kind));
    }

    [Test]
    public async Task Cannot_Create_User_With_Duplicate_Email()
    {
        var email = "test@test.com";
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var initialUserCreateModel = new UserCreateModel
        {
            UserName = "Test1",
            Email = email,
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.That(result.Success, Is.True);

        var duplicateUserCreateModel = new UserCreateModel
        {
            UserName = "Test2",
            Email = email,
            Name = "Duplicate Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var secondResult = await userService.CreateAsync(Constants.Security.SuperUserKey, duplicateUserCreateModel, true);
        Assert.That(secondResult.Success, Is.False);
        Assert.That(secondResult.Status, Is.EqualTo(UserOperationStatus.DuplicateEmail));
    }

    [Test]
    public async Task Cannot_Create_User_With_Duplicate_UserName()
    {
        var userName = "UserName";
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var initialUserCreateModel = new UserCreateModel
        {
            UserName = userName,
            Email = "test@email.com",
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.That(result.Success, Is.True);

        var duplicateUserCreateModel = new UserCreateModel
        {
            UserName = userName,
            Email = "another@email.com",
            Name = "Duplicate Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var secondResult = await userService.CreateAsync(Constants.Security.SuperUserKey, duplicateUserCreateModel, true);
        Assert.That(secondResult.Success, Is.False);
        Assert.That(secondResult.Status, Is.EqualTo(UserOperationStatus.DuplicateUserName));
    }

    [Test]
    public async Task Cannot_Create_User_Without_User_Group()
    {
        UserCreateModel userCreateModel = new UserCreateModel
        {
            UserName = "NoUser@Group.com",
            Email = "NoUser@Group.com",
            Name = "NoUser@Group.com",
        };

        IUserService userService = CreateUserService();

        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserOperationStatus.NoUserGroup));
    }

    [Test]
    public async Task Performing_User_Must_Exist_When_Creating()
    {
        IUserService userService = CreateUserService();

        var result = await userService.CreateAsync(Guid.Empty, new UserCreateModel(), true);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(UserOperationStatus.MissingUser));
    }
}
