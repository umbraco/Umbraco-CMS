using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class UserServiceCrudTests
{
    [TestCase(null, 1)]                     // Requesting no filter, will just get the admin user but not the created and disabled one.
                                            //  - verifies fix for https://github.com/umbraco/Umbraco-CMS/issues/18812
    [TestCase(UserState.Inactive, 1)]       // Requesting inactive, will just get the admin user but not the created and disabled one.
    [TestCase(UserState.Disabled, 0)]       // Requesting disabled, won't get any as admin user isn't disabled and, whilst the created one is, disabled users are hidden.
    [TestCase(UserState.All, 1)]            // Requesting all, will just get the admin user but not the created and disabled one.
    public async Task Cannot_Request_Disabled_If_Hidden(UserState? includeState, int expectedCount)
    {
        var userService = CreateUserService(new SecuritySettings { HideDisabledUsersInBackOffice = true });
        var editorGroup = await UserGroupService.GetAsync(Constants.Security.EditorGroupKey);

        var createModel = new UserCreateModel
        {
            UserName = "editor@mail.com",
            Email = "editor@mail.com",
            Name = "Editor",
            UserGroupKeys = new HashSet<Guid> { editorGroup.Key },
        };

        var createAttempt = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.That(createAttempt.Success, Is.True);

        var disableStatus =
            await userService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { createAttempt.Result.CreatedUser!.Key });
        Assert.That(disableStatus, Is.EqualTo(UserOperationStatus.Success));

        var filter = new UserFilter();
        if (includeState.HasValue)
        {
            filter.IncludeUserStates = new HashSet<UserState> { includeState.Value };
        }

        var filterAttempt = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);
        Assert.That(filterAttempt.Success, Is.True);
        Assert.That(filterAttempt.Result.Items.Count(), Is.EqualTo(expectedCount));
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
        Assert.That(createEditorAttempt.Success, Is.True);

        var editor = createEditorAttempt.Result.CreatedUser;

        // An empty filter is essentially the same as "Give me everything" but you still can't see super users.
        var filter = new UserFilter();
        var filterAttempt = await userService.FilterAsync(editor!.Key, filter, 0, 10000);

        Assert.That(filterAttempt.Success, Is.True);
        var result = filterAttempt.Result;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(1));
        var onlyUser = result.Items.First();
        Assert.That(onlyUser.Key, Is.EqualTo(editor.Key));
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
        Assert.That(createEditorAttempt.Success, Is.True);

        var filter = new UserFilter {NameFilters = new HashSet<string> {"admin"}};

        var filterAttempt = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 10000);
        Assert.That(filterAttempt.Success, Is.True);
        var result = filterAttempt.Result;

        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.FirstOrDefault(x => x.Key == Constants.Security.SuperUserKey), Is.Not.Null);
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

        Assert.That(createEditorAttempt.Success, Is.True);
        Assert.That(createAdminAttempt.Success, Is.True);

        var filter = new UserFilter {IncludedUserGroups = new HashSet<Guid> {adminGroup!.Key}};

        var editorFilterAttempt =
            await userService.FilterAsync(createEditorAttempt.Result.CreatedUser!.Key, filter, 0, 10000);
        Assert.That(editorFilterAttempt.Success, Is.True);
        var editorAllUsers = editorFilterAttempt.Result.Items.ToList();
        Assert.That(editorAllUsers, Is.Empty);
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

        Assert.That(createEditorAttempt.Success, Is.True);
        Assert.That(createAdminAttempt.Success, Is.True);

        var filter = new UserFilter {IncludedUserGroups = new HashSet<Guid> {adminGroup!.Key}};

        var adminFilterAttempt =
            await userService.FilterAsync(createAdminAttempt.Result.CreatedUser!.Key, filter, 0, 10000);
        Assert.That(adminFilterAttempt.Success, Is.True);
        var adminAllUsers = adminFilterAttempt.Result.Items.ToList();
        Assert.That(adminAllUsers, Has.Count.EqualTo(1));
        Assert.That(adminAllUsers.FirstOrDefault(x => x.Key == createAdminAttempt.Result.CreatedUser!.Key), Is.Not.Null);
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
            Assert.That(result.Success, Is.True);
        }
    }

    [Test]
    public async Task Can_Include_User_Groups()
    {
        var userService = CreateUserService();
        await CreateTestUsers(userService);

        var writerGroup = await UserGroupService.GetAsync(DatabaseDataCreator.WriterGroupAlias);
        var filter = new UserFilter
        {
            IncludedUserGroups = new HashSet<Guid> { writerGroup!.Key }
        };

        var onlyWritesResult = await userService.FilterAsync(Constants.Security.SuperUserKey, filter, 0, 1000);

        Assert.That(onlyWritesResult.Success, Is.True);
        var users = onlyWritesResult.Result.Items.ToList();
        Assert.That(users.Any(), Is.True);
        Assert.That(users.Any(x => x.Groups.FirstOrDefault(y => y.Key == writerGroup.Key) is null), Is.False);
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
        Assert.That((bool)noEditorResult, Is.True);
        var users = noEditorResult.Result.Items.ToList();
        Assert.That(users.Any(), Is.True);
        Assert.That(users.Any(x => x.Groups.FirstOrDefault(y => y.Key == editorGroup.Key) is not null), Is.False);
    }
}


