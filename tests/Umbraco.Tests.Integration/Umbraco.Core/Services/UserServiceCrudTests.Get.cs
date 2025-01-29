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
    public async Task Only_Super_User_Can_Get_Super_user()
    {
        var userService = CreateUserService();
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);

        var nonSuperCreateModel = new UserCreateModel
        {
            Email = "not@super.com",
            UserName = "not@super.com",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key, adminGroup.Key },
            Name = "Not A Super User"
        };

        var createEditorAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, nonSuperCreateModel, true);
        Assert.IsTrue(createEditorAttempt.Success);

        var editor = createEditorAttempt.Result.CreatedUser;
        var allUsersAttempt = await userService.GetAllAsync(editor!.Key, 0, 10000);

        Assert.IsTrue(allUsersAttempt.Success);
        var result = allUsersAttempt.Result;
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(1, result.Total);
        var onlyUser = result.Items.First();
        Assert.AreEqual(editor.Key, onlyUser.Key);
    }

    [Test]
    public async Task Super_User_Can_See_Super_User()
    {
        var userService = CreateUserService();
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var nonSuperCreateModel = new UserCreateModel
        {
            Email = "not@super.com",
            UserName = "not@super.com",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
            Name = "Not A Super User"
        };

        var createEditorAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, nonSuperCreateModel, true);
        Assert.IsTrue(createEditorAttempt.Success);

        var editor = createEditorAttempt.Result.CreatedUser;
        var allUsersAttempt = await userService.GetAllAsync(Constants.Security.SuperUserKey, 0, 10000);
        Assert.IsTrue(allUsersAttempt.Success);
        var result = allUsersAttempt.Result;

        Assert.AreEqual(2, result.Items.Count());
        Assert.AreEqual(2, result.Total);
        Assert.IsTrue(result.Items.Any(x => x.Key == Constants.Security.SuperUserKey));
        Assert.IsTrue(result.Items.Any(x => x.Key == editor!.Key));
    }

    [Test]
    public async Task Non_Admins_Cannot_Get_admins()
    {
        var userService = CreateUserService();
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var editorCreateModel = new UserCreateModel
        {
            UserName = "editor@mail.com",
            Email = "editor@mail.com",
            Name = "Editor Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
        };

        var adminCreateModel = new UserCreateModel
        {
            UserName = "admin@mail.com",
            Email = "admin@mail.com",
            Name = "Admin Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key, adminGroup.Key },
        };

        var createEditorAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, editorCreateModel, true);
        var createAdminAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, adminCreateModel, true);

        Assert.IsTrue(createEditorAttempt.Success);
        Assert.IsTrue(createAdminAttempt.Success);

        var editorAllUsersAttempt = await userService.GetAllAsync(createEditorAttempt.Result.CreatedUser!.Key, 0, 10000);
        Assert.IsTrue(editorAllUsersAttempt.Success);
        var editorAllUsers = editorAllUsersAttempt.Result.Items.ToList();
        Assert.AreEqual(1, editorAllUsers.Count);
        Assert.AreEqual(createEditorAttempt.Result.CreatedUser!.Key, editorAllUsers.First().Key);
    }

    [Test]
    public async Task Admins_Can_See_Admins()
    {
        var userService = CreateUserService();
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var editorCreateModel = new UserCreateModel
        {
            UserName = "editor@mail.com",
            Email = "editor@mail.com",
            Name = "Editor Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
        };

        var adminCreateModel = new UserCreateModel
        {
            UserName = "admin@mail.com",
            Email = "admin@mail.com",
            Name = "Admin Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key, adminGroup.Key },
        };

        var createEditorAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, editorCreateModel, true);
        var createAdminAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, adminCreateModel, true);

        Assert.IsTrue(createEditorAttempt.Success);
        Assert.IsTrue(createAdminAttempt.Success);

        var adminAllUsersAttempt = await userService.GetAllAsync(createAdminAttempt.Result.CreatedUser!.Key, 0, 10000);
        Assert.IsTrue(adminAllUsersAttempt.Success);
        var adminAllUsers = adminAllUsersAttempt.Result.Items.ToList();
        Assert.AreEqual(2, adminAllUsers.Count);
        Assert.IsTrue(adminAllUsers.Any(x => x.Key == createEditorAttempt.Result.CreatedUser!.Key));
        Assert.IsTrue(adminAllUsers.Any(x => x.Key == createAdminAttempt.Result.CreatedUser!.Key));
    }

    [Test]
    public async Task Cannot_See_Disabled_When_HideDisabled_Is_True()
    {
        var userService = CreateUserService(securitySettings: new SecuritySettings { HideDisabledUsersInBackOffice = true });
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var firstEditorCreateModel = new UserCreateModel
        {
            UserName = "firstEditor@mail.com",
            Email = "firstEditor@mail.com",
            Name = "First Editor",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
        };

        var firstEditorResult = await userService.CreateAsync(Constants.Security.SuperUserKey, firstEditorCreateModel, true);
        Assert.IsTrue(firstEditorResult.Success);

        var secondEditorCreateModel = new UserCreateModel
        {
            UserName = "secondEditor@mail.com",
            Email = "secondEditor@mail.com",
            Name = "Second Editor",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
        };

        var secondEditorResult = await userService.CreateAsync(Constants.Security.SuperUserKey, secondEditorCreateModel, true);
        Assert.IsTrue(secondEditorResult.Success);

        var disableStatus = await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid>{ secondEditorResult.Result.CreatedUser!.Key });
        Assert.AreEqual(disableStatus, UserOperationStatus.Success);

        var allUsersAttempt = await userService.GetAllAsync(Constants.Security.SuperUserKey, 0, 10000);
        Assert.IsTrue(allUsersAttempt.Success);
        var allUsers = allUsersAttempt.Result!.Items.ToList();
        Assert.AreEqual(2, allUsers.Count);
        Assert.IsTrue(allUsers.Any(x => x.Key == firstEditorResult.Result.CreatedUser!.Key));
        Assert.IsTrue(allUsers.Any(x => x.Key == Constants.Security.SuperUserKey));
    }

    [Test]
    public async Task Requesting_User_Must_Exist_When_Calling_Get_All()
    {
        var userService = CreateUserService();

        var getAllAttempt = await userService.GetAllAsync(Guid.NewGuid(), 0, 10000);
        Assert.IsFalse(getAllAttempt.Success);
        Assert.AreEqual(UserOperationStatus.MissingUser, getAllAttempt.Status);
        Assert.IsNull(getAllAttempt.Result);
    }
}
