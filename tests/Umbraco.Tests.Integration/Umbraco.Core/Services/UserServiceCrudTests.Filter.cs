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
    [TestCase(UserState.Disabled)]
    [TestCase(UserState.All)]
    public async Task Cannot_Request_Disabled_If_Hidden(UserState includeState)
    {
        var userService = CreateUserService(new SecuritySettings {HideDisabledUsersInBackOffice = true});
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var createModel = new UserCreateModel
        {
            UserName = "editor@mail.com",
            Email = "editor@mail.com",
            Name = "Editor",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key }
        };

        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(createAttempt.Success);

        var disableStatus =
            await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid>{ createAttempt.Result.CreatedUser!.Key });
        Assert.AreEqual(UserOperationStatus.Success, disableStatus);

        var filter = new UserFilter {IncludeUserStates = new HashSet<UserState> {includeState}};

        var filterAttempt = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);
        Assert.IsTrue(filterAttempt.Success);
        Assert.AreEqual(0, filterAttempt.Result.Items.Count());
    }

    [Test]
    public async Task Only_Super_User_Can_Filter_Super_user()
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

        var createEditorAttempt =
            await userService.CreateAsync(Constants.Security.SuperUserKey, nonSuperCreateModel, true);
        Assert.IsTrue(createEditorAttempt.Success);

        var editor = createEditorAttempt.Result.CreatedUser;

        // An empty filter is essentially the same as "Give me everything" but you still can't see super users.
        var filter = new UserFilter();
        var filterAttempt = await userService.FilterAsync(editor!.Key, filter, 0, 10000);

        Assert.IsTrue(filterAttempt.Success);
        var result = filterAttempt.Result;
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(1, result.Total);
        var onlyUser = result.Items.First();
        Assert.AreEqual(editor.Key, onlyUser.Key);
    }

    [Test]
    public async Task Super_User_Can_Filter_Super_User()
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

        var createEditorAttempt =
            await userService.CreateAsync(Constants.Security.SuperUserKey, nonSuperCreateModel, true);
        Assert.IsTrue(createEditorAttempt.Success);

        var filter = new UserFilter {NameFilters = new HashSet<string> {"admin"}};

        var filterAttempt = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 10000);
        Assert.IsTrue(filterAttempt.Success);
        var result = filterAttempt.Result;

        Assert.AreEqual(1, result.Items.Count());
        Assert.AreEqual(1, result.Total);
        Assert.IsNotNull(result.Items.FirstOrDefault(x => x.Key == Constants.Security.SuperUserKey));
    }

    [Test]
    public async Task Non_Admins_Cannot_Filter_Admins()
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

        var createEditorAttempt =
            await userService.CreateAsync(Constants.Security.SuperUserKey, editorCreateModel, true);
        var createAdminAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, adminCreateModel, true);

        Assert.IsTrue(createEditorAttempt.Success);
        Assert.IsTrue(createAdminAttempt.Success);

        var filter = new UserFilter {IncludedUserGroups = new HashSet<Guid> {adminGroup!.Key}};

        var editorFilterAttempt =
            await userService.FilterAsync(createEditorAttempt.Result.CreatedUser!.Key, filter, 0, 10000);
        Assert.IsTrue(editorFilterAttempt.Success);
        var editorAllUsers = editorFilterAttempt.Result.Items.ToList();
        Assert.AreEqual(0, editorAllUsers.Count);
    }

    [Test]
    public async Task Admins_Can_Filter_Admins()
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

        var createEditorAttempt =
            await userService.CreateAsync(Constants.Security.SuperUserKey, editorCreateModel, true);
        var createAdminAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, adminCreateModel, true);

        Assert.IsTrue(createEditorAttempt.Success);
        Assert.IsTrue(createAdminAttempt.Success);

        var filter = new UserFilter {IncludedUserGroups = new HashSet<Guid> {adminGroup!.Key}};

        var adminFilterAttempt =
            await userService.FilterAsync(createAdminAttempt.Result.CreatedUser!.Key, filter, 0, 10000);
        Assert.IsTrue(adminFilterAttempt.Success);
        var adminAllUsers = adminFilterAttempt.Result.Items.ToList();
        Assert.AreEqual(1, adminAllUsers.Count);
        Assert.IsNotNull(adminAllUsers.FirstOrDefault(x => x.Key == createAdminAttempt.Result.CreatedUser!.Key));
    }


    private async Task CreateTestUsers(IUserService userService)
    {
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);
        var writerGroup = await UserGroupService.GetAsync(Constants.Security.WriterGroupKey);
        var translatorGroup = await UserGroupService.GetAsync(Constants.Security.TranslatorGroupKey);

        var createModels = new List<UserCreateModel>
        {
            new()
            {
                UserName = "editor@email.com",
                Email = "editor@email.com",
                Name = "Editor",
                UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
            },
            new()
            {
                UserName = "admin@email.com",
                Email = "admin@email.com",
                Name = "Admin",
                UserGroupKeys = new HashSet<Guid> { adminGroup.Key },
            },
            new()
            {
                UserName = "write@email.com",
                Email = "write@email.com",
                Name = "Write",
                UserGroupKeys = new HashSet<Guid> { writerGroup.Key },
            },
            new()
            {
                UserName = "translator@email.com",
                Email = "translator@email.com",
                Name = "Translator",
                UserGroupKeys = new HashSet<Guid> { translatorGroup.Key },
            },
            new()
            {
                UserName = "EverythingButAdmin@email.com",
                Email = "EverythingButAdmin@email.com",
                Name = "Everything But Admin",
                UserGroupKeys = new HashSet<Guid> { editorGroup.Key, writerGroup.Key, translatorGroup.Key },
            }
        };

        foreach (var model in createModels)
        {
            var result = await userService.CreateAsync(Constants.Security.SuperUserKey, model);
            Assert.IsTrue(result.Success);
        }
    }

    [Test]
    public async Task Can_Include_User_Groups()
    {
        var userService = CreateUserService();
        await CreateTestUsers(userService);

        var writerGroup = await UserGroupService.GetAsync(Constants.Security.WriterGroupAlias);
        var filter = new UserFilter
        {
            IncludedUserGroups = new HashSet<Guid> { writerGroup!.Key }
        };

        var onlyWritesResult = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);

        Assert.IsTrue(onlyWritesResult.Success);
        var users = onlyWritesResult.Result.Items.ToList();
        Assert.IsTrue(users.Any());
        Assert.IsFalse(users.Any(x => x.Groups.FirstOrDefault(y => y.Key == writerGroup.Key) is null));
    }

    [Test]
    public async Task Can_Exclude_User_Groups()
    {
        var userService = CreateUserService();
        await CreateTestUsers(userService);

        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);
        var filter = new UserFilter
        {
            ExcludeUserGroups = new HashSet<Guid> { editorGroup!.Key }
        };

        var noEditorResult = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);
        Assert.IsTrue(noEditorResult);
        var users = noEditorResult.Result.Items.ToList();
        Assert.IsTrue(users.Any());
        Assert.IsFalse(users.Any(x => x.Groups.FirstOrDefault(y => y.Key == editorGroup.Key) is not null));
    }
}


