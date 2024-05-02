using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class UserGroupServiceTests
{
    [TestCase("one", "two", "three")]
    [TestCase("two", "three")]
    [TestCase("three")]
    [TestCase]
    public async Task Filter_Returns_Only_User_Groups_For_Non_Admin(params string[] userGroupAliases)
    {
        var userKey = Guid.NewGuid();
        var userGroupService = SetupUserGroupService(userKey, userGroupAliases);

        var result = await userGroupService.FilterAsync(userKey, null, 0, 10);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(userGroupAliases.Length, result.Result.Items.Count());
            foreach (var userGroupAlias in userGroupAliases)
            {
                Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == userGroupAlias));
            }
        });
    }

    [TestCase("four", "five", "six")]
    [TestCase("four")]
    [TestCase]
    public async Task Filter_Does_Not_Return_Non_Existing_Groups(params string[] userGroupAliases)
    {
        var userKey = Guid.NewGuid();
        var userGroupService = SetupUserGroupService(userKey, userGroupAliases);

        var result = await userGroupService.FilterAsync(userKey, null, 0, 10);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Result.Items);
        });
    }

    [Test]
    public async Task Filter_Returns_All_Groups_For_Admin()
    {
        var userKey = Guid.NewGuid();
        var userGroupService = SetupUserGroupService(userKey, new [] { Constants.Security.AdminGroupAlias });

        var result = await userGroupService.FilterAsync(userKey, null, 0, 10);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.Result.Items.Count());
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == Constants.Security.AdminGroupAlias));
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "one"));
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "two"));
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "three"));
        });
    }

    [Test]
    public async Task Filter_Can_Filter_By_Group_Name()
    {
        var userKey = Guid.NewGuid();
        var userGroupService = SetupUserGroupService(userKey, new [] { Constants.Security.AdminGroupAlias });

        var result = await userGroupService.FilterAsync(userKey, "e", 0, 10);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Result.Items.Count());
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "one"));
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "three"));
        });
    }

    private IEnumerable<IReadOnlyUserGroup> CreateGroups(params string[] aliases)
        => aliases.Select(alias =>
        {
            var group = new Mock<IReadOnlyUserGroup>();
            group.SetupGet(g => g.Alias).Returns(alias);
            return group.Object;
        }).ToArray();

    private IUserGroupService SetupUserGroupService(Guid userKey, string[] userGroupAliases)
    {
        var user = new Mock<IUser>();
        user.SetupGet(u => u.Key).Returns(userKey);
        user.Setup(u => u.Groups).Returns(CreateGroups(userGroupAliases));

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetAsync(userKey)).Returns(Task.FromResult(user.Object));

        var userGroupRepository = new Mock<IUserGroupRepository>();
        userGroupRepository
            .Setup(r => r.GetMany())
            .Returns(new[]
            {
                new UserGroup(Mock.Of<IShortStringHelper>(), 0, Constants.Security.AdminGroupAlias, "Administrators", null),
                new UserGroup(Mock.Of<IShortStringHelper>(), 0, "one", "Group One", null),
                new UserGroup(Mock.Of<IShortStringHelper>(), 0, "two", "Group Two", null),
                new UserGroup(Mock.Of<IShortStringHelper>(), 0, "three", "Group Three", null),
            });

        return new UserGroupService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            userGroupRepository.Object,
            Mock.Of<IUserGroupPermissionService>(),
            Mock.Of<IEntityService>(),
            userService.Object,
            Mock.Of<ILogger<UserGroupService>>());
    }
}
