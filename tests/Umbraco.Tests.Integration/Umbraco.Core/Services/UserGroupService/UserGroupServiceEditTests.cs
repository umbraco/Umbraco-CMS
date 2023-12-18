using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services.UserGroupService;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserGroupServiceEditTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task Edit_UserGroup_WithUsers_AddsGroup_To_UserUserGroups_Removes_FromOthers()
    {
        var editorGroupAlias = "editor";
        var testGroupAlias = "testGroup";

        // get the editor group
        var editorUserGroup = await UserGroupService.GetAsync(editorGroupAlias);
        var newUserGroupKeys = new HashSet<Guid>() { editorUserGroup!.Key };

        // create 3 new users
        var create1Result = await CreateTestMember(newUserGroupKeys, "user1@test.test");
        var create2Result = await CreateTestMember(newUserGroupKeys, "user2@test.test");
        var create3Result = await CreateTestMember(newUserGroupKeys, "user3@test.test");

        // create a new group with first user
        var userGroupCreateResult = await UserGroupService.CreateAsync(
            new UserGroup(ShortStringHelper) { Name = "Test Group", Alias = testGroupAlias },
            Constants.Security.SuperUserKey,
            new[] { create1Result.Result.CreatedUser!.Key });

        // Add the group to the second user
        var user2ToEdit = await UserService.GetAsync(create2Result.Result.CreatedUser!.Key);
        user2ToEdit!.AddGroup(userGroupCreateResult.Result.ToReadOnlyGroup());
        UserService.Save(user2ToEdit);

        // update the userGroup and set the assigned users to user3
        userGroupCreateResult.Result.Name = "Updated Test Group";
        await UserGroupService.UpdateAsync(
            new UserGroupUpdateModel(
            userGroupCreateResult.Result,
            new[] { create3Result.Result.CreatedUser!.Key }),
            Constants.Security.SuperUserKey);

        // check only the first member is added to the userGroup and retains its other groups
        var user1 = await UserService.GetAsync(create1Result.Result.CreatedUser.Key);
        var user2 = await UserService.GetAsync(create2Result.Result.CreatedUser.Key);
        var user3 = await UserService.GetAsync(create3Result.Result.CreatedUser!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, user1!.Groups.Count());
            Assert.AreEqual(1, user2!.Groups.Count());
            Assert.AreEqual(2, user3!.Groups.Count());

            Assert.IsTrue(user1.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user2.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user3.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);

            Assert.IsTrue(
                user3.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) ==
                1);
        });
    }

    [Test]
    public async Task Edit_UserGroup_With_EmptyUsers_Removes_FromAll()
    {
        var editorGroupAlias = "editor";
        var testGroupAlias = "testGroup";

        // get the editor group
        var editorUserGroup = await UserGroupService.GetAsync(editorGroupAlias);
        var newUserGroupKeys = new HashSet<Guid>() { editorUserGroup!.Key };

        // create 3 new users
        var create1Result = await CreateTestMember(newUserGroupKeys, "user1@test.test");
        var create2Result = await CreateTestMember(newUserGroupKeys, "user2@test.test");
        var create3Result = await CreateTestMember(newUserGroupKeys, "user3@test.test");

        // create a new group with first user
        var userGroupCreateResult = await UserGroupService.CreateAsync(
            new UserGroup(ShortStringHelper) { Name = "Test Group", Alias = testGroupAlias },
            Constants.Security.SuperUserKey,
            new[] { create1Result.Result.CreatedUser!.Key });

        // Add the group to the second user
        var user2ToEdit = await UserService.GetAsync(create2Result.Result.CreatedUser!.Key);
        user2ToEdit!.AddGroup(userGroupCreateResult.Result.ToReadOnlyGroup());
        UserService.Save(user2ToEdit);

        // update the userGroup and clear the assignment list
        userGroupCreateResult.Result.Name = "Updated Test Group";
        await UserGroupService.UpdateAsync(
            new UserGroupUpdateModel(
                userGroupCreateResult.Result,
                Array.Empty<Guid>()),
            Constants.Security.SuperUserKey);

        // check no user has the group assigned
        var user1 = await UserService.GetAsync(create1Result.Result.CreatedUser.Key);
        var user2 = await UserService.GetAsync(create2Result.Result.CreatedUser.Key);
        var user3 = await UserService.GetAsync(create3Result.Result.CreatedUser!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, user1!.Groups.Count());
            Assert.AreEqual(1, user2!.Groups.Count());
            Assert.AreEqual(1, user3!.Groups.Count());

            Assert.IsTrue(user1.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user2.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user3.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
        });
    }

    [Test]
    public async Task Edit_UserGroup_With_NullUsers_DoesNotAlter_GroupsOnUsers_WithoutModel()
    {
        var editorGroupAlias = "editor";
        var testGroupAlias = "testGroup";

        // get the editor group
        var editorUserGroup = await UserGroupService.GetAsync(editorGroupAlias);
        var newUserGroupKeys = new HashSet<Guid>() { editorUserGroup!.Key };

        // create 3 new users
        var create1Result = await CreateTestMember(newUserGroupKeys, "user1@test.test");
        var create2Result = await CreateTestMember(newUserGroupKeys, "user2@test.test");
        var create3Result = await CreateTestMember(newUserGroupKeys, "user3@test.test");

        // create a new group with first user
        var userGroupCreateResult = await UserGroupService.CreateAsync(
            new UserGroup(ShortStringHelper) { Name = "Test Group", Alias = testGroupAlias },
            Constants.Security.SuperUserKey,
            new[] { create1Result.Result.CreatedUser!.Key });

        // Add the group to the second
        var user2ToEdit = await UserService.GetAsync(create2Result.Result.CreatedUser!.Key);
        user2ToEdit!.AddGroup(userGroupCreateResult.Result.ToReadOnlyGroup());
        UserService.Save(user2ToEdit);

        // update the userGroup without supplying a new list of users
        userGroupCreateResult.Result.Name = "Updated Test Group";
        await UserGroupService.UpdateAsync(
            new UserGroupUpdateModel(
                userGroupCreateResult.Result,
                null),
            Constants.Security.SuperUserKey);

        // check no usergroup changes occured
        var user1 = await UserService.GetAsync(create1Result.Result.CreatedUser.Key);
        var user2 = await UserService.GetAsync(create2Result.Result.CreatedUser.Key);
        var user3 = await UserService.GetAsync(create3Result.Result.CreatedUser!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, user1!.Groups.Count());
            Assert.AreEqual(2, user2!.Groups.Count());
            Assert.AreEqual(1, user3!.Groups.Count());

            Assert.IsTrue(user1.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user2.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user3.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);

            Assert.IsTrue(
                user1.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) ==
                1);
            Assert.IsTrue(
                user2.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) ==
                1);
        });
    }

    [Test]
    public async Task Edit_UserGroup_With_NullUsers_DoesNotAlter_GroupsOnUsers()
    {
        var editorGroupAlias = "editor";
        var testGroupAlias = "testGroup";

        // get the editor group
        var editorUserGroup = await UserGroupService.GetAsync(editorGroupAlias);
        var newUserGroupKeys = new HashSet<Guid>() { editorUserGroup!.Key };

        // create 3 new users
        var create1Result = await CreateTestMember(newUserGroupKeys, "user1@test.test");
        var create2Result = await CreateTestMember(newUserGroupKeys, "user2@test.test");
        var create3Result = await CreateTestMember(newUserGroupKeys, "user3@test.test");

        // create a new group with first user
        var userGroupCreateResult = await UserGroupService.CreateAsync(
            new UserGroup(ShortStringHelper) { Name = "Test Group", Alias = testGroupAlias },
            Constants.Security.SuperUserKey,
            new[] { create1Result.Result.CreatedUser!.Key });

        // Add the group to the second
        var user2ToEdit = await UserService.GetAsync(create2Result.Result.CreatedUser!.Key);
        user2ToEdit!.AddGroup(userGroupCreateResult.Result.ToReadOnlyGroup());
        UserService.Save(user2ToEdit);

        // update the userGroup without supplying a new list of users
        userGroupCreateResult.Result.Name = "Updated Test Group";
        await UserGroupService.UpdateAsync(
            userGroupCreateResult.Result,
            Constants.Security.SuperUserKey);

        // check no usergroup changes occured
        var user1 = await UserService.GetAsync(create1Result.Result.CreatedUser.Key);
        var user2 = await UserService.GetAsync(create2Result.Result.CreatedUser.Key);
        var user3 = await UserService.GetAsync(create3Result.Result.CreatedUser!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, user1!.Groups.Count());
            Assert.AreEqual(2, user2!.Groups.Count());
            Assert.AreEqual(1, user3!.Groups.Count());

            Assert.IsTrue(user1.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user2.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
            Assert.IsTrue(user3.Groups.Count(g =>
                g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);

            Assert.IsTrue(
                user1.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) ==
                1);
            Assert.IsTrue(
                user2.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) ==
                1);
        });
    }

    private async Task<Attempt<UserCreationResult, UserOperationStatus>> CreateTestMember(HashSet<Guid> initialGroups,
        string emailNameUserName)
    {
        return await UserService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = emailNameUserName,
                Name = emailNameUserName,
                UserGroupKeys = initialGroups,
                UserName = emailNameUserName,
            },
            approveUser: true);
    }
}
