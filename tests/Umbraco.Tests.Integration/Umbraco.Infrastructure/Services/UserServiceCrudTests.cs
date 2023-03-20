using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserServiceCrudTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService CreateUserService(SecuritySettings securitySettings = null)
    {
        securitySettings ??= GetRequiredService<IOptions<SecuritySettings>>().Value;
        IOptions<SecuritySettings> securityOptions = Options.Create(securitySettings);

        return new UserService(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILoggerFactory>(),
            GetRequiredService<IEventMessagesFactory>(),
            GetRequiredService<IUserRepository>(),
            GetRequiredService<IUserGroupRepository>(),
            GetRequiredService<IOptions<GlobalSettings>>(),
            securityOptions,
            GetRequiredService<UserEditorAuthorizationHelper>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<IEntityService>(),
            GetRequiredService<ILocalLoginSettingProvider>(),
            GetRequiredService<IUserInviteSender>(),
            GetRequiredService<MediaFileManager>(),
            GetRequiredService<ITemporaryFileService>(),
            GetRequiredService<IShortStringHelper>(),
            GetRequiredService<IOptions<ContentSettings>>());
    }

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
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, creationModel, true);

        if (shouldSucceed is false)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UserOperationStatus.UserNameIsNotEmail, result.Status);
            return;
        }

        Assert.IsTrue(result.Success);
        Assert.AreEqual(UserOperationStatus.Success, result.Status);
        var createdUser = result.Result.CreatedUser;
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(username, createdUser.Username);
        Assert.AreEqual(email, createdUser.Email);
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
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.IsTrue(result.Success);

        var duplicateUserCreateModel = new UserCreateModel
        {
            UserName = "Test2",
            Email = email,
            Name = "Duplicate Mc. Gee",
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var secondResult = await userService.CreateAsync(Constants.Security.SuperUserKey, duplicateUserCreateModel, true);
        Assert.IsFalse(secondResult.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateEmail, secondResult.Status);
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
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var userService = CreateUserService(new SecuritySettings { UsernameIsEmail = false });
        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, initialUserCreateModel, true);
        Assert.IsTrue(result.Success);

        var duplicateUserCreateModel = new UserCreateModel
        {
            UserName = userName,
            Email = "another@email.com",
            Name = "Duplicate Mc. Gee",
            UserGroups = new SortedSet<IUserGroup> { userGroup! }
        };

        var secondResult = await userService.CreateAsync(Constants.Security.SuperUserKey, duplicateUserCreateModel, true);
        Assert.IsFalse(secondResult.Success);
        Assert.AreEqual(UserOperationStatus.DuplicateUserName, secondResult.Status);
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

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.NoUserGroup, result.Status);
    }
}
