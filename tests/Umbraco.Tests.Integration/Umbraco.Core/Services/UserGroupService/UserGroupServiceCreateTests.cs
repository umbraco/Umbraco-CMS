using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services.UserGroupService;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserGroupServiceCreateTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task Create_UserGroup_With_Users_Adds_Group_To_User_UserGroups()
    {

        var editorGroupAlias = "editor";
        var testGroupAlias = "testGroup";

        // get the editor group
        var editorUserGroup = await UserGroupService.GetAsync(editorGroupAlias);
        var newUserGroupKeys = new HashSet<Guid>() { editorUserGroup!.Key };

        // create 3 new users
        var create1Result = await UserService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = "user1@test.test",
                Name = "user1@test.test",
                UserGroupKeys = newUserGroupKeys,
                UserName = "user1@test.test",
            },
            approveUser: true);

        var create2Result = await UserService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = "user2@test.test",
                Name = "user2@test.test",
                UserGroupKeys = newUserGroupKeys,
                UserName = "user2@test.test",
            },
            approveUser: true);

        var create3Result = await UserService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = "user3@test.test",
                Name = "user3@test.test",
                UserGroupKeys = newUserGroupKeys,
                UserName = "user3@test.test",
            },
            approveUser: true);

        // create a new group with first member
        await UserGroupService.CreateAsync(
            new UserGroup(ShortStringHelper) { Name = "Test Group", Alias = testGroupAlias },
            Constants.Security.SuperUserKey,
            new[] { create1Result.Result.CreatedUser!.Key });

        // check only the first member is added to the usergroup and retains its other groups
        var user1 = await UserService.GetAsync(create1Result.Result.CreatedUser.Key);
        var user2 = await UserService.GetAsync(create2Result.Result.CreatedUser!.Key);
        var user3 = await UserService.GetAsync(create3Result.Result.CreatedUser!.Key);

        Assert.AreEqual(user1!.Groups.Count(), 2);
        Assert.AreEqual(user2!.Groups.Count(), 1);
        Assert.AreEqual(user3!.Groups.Count(), 1);

        Assert.IsTrue(user1.Groups.Count(g =>
            g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
        Assert.IsTrue(user2.Groups.Count(g =>
            g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
        Assert.IsTrue(user3.Groups.Count(g =>
            g.Alias.Equals(editorGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);

        Assert.IsTrue(
            user1.Groups.Count(g => g.Alias.Equals(testGroupAlias, StringComparison.InvariantCultureIgnoreCase)) == 1);
    }
}
