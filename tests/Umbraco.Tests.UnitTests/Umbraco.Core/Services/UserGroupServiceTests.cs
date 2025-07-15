using System.Data;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations.Install;

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
        var userGroupService = SetupUserGroupServiceWithUserAndGetManyReturnsFourGroups(userKey, userGroupAliases);

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
        var userGroupService = SetupUserGroupServiceWithUserAndGetManyReturnsFourGroups(userKey, userGroupAliases);

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
        var userGroupService = SetupUserGroupServiceWithUserAndGetManyReturnsFourGroups(userKey, new [] { Constants.Security.AdminGroupAlias });

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
        var userGroupService = SetupUserGroupServiceWithUserAndGetManyReturnsFourGroups(userKey, new [] { Constants.Security.AdminGroupAlias });

        var result = await userGroupService.FilterAsync(userKey, "e", 0, 10);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Result.Items.Count());
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "one"));
            Assert.IsNotNull(result.Result.Items.SingleOrDefault(g => g.Alias == "three"));
        });
    }

    [TestCase(null, null, UserGroupOperationStatus.Success)]
    [TestCase(Constants.Security.AdminGroupKeyString, Constants.Security.AdminGroupAlias, UserGroupOperationStatus.CanNotUpdateAliasIsSystemUserGroup)]
    [TestCase(Constants.Security.SensitiveDataGroupKeyString, DatabaseDataCreator.SensitiveDataGroupAlias, UserGroupOperationStatus.CanNotUpdateAliasIsSystemUserGroup)]
    [TestCase(Constants.Security.TranslatorGroupString, DatabaseDataCreator.TranslatorGroupAlias, UserGroupOperationStatus.CanNotUpdateAliasIsSystemUserGroup)]
    public async Task Can_Not_Update_SystemGroup_Alias(string? systemGroupKey, string? systemGroupAlias, UserGroupOperationStatus status)
    {
        // prep
        var userGroupAlias = systemGroupAlias ?? "someNonSystemAlias";
        Guid userGroupKey = systemGroupKey is not null ? new Guid(systemGroupKey) : Guid.NewGuid();

        // Arrange
        var actingUserKey = Guid.NewGuid();
        var mockUser = SetupUserWithGroupAccess(actingUserKey, [Constants.Security.AdminGroupAlias]);
        var userService = SetupUserServiceWithGetUserByKey(actingUserKey, mockUser);
        var userGroupRepository = new Mock<IUserGroupRepository>();
        var persistedUserGroup =
            new UserGroup(
                Mock.Of<IShortStringHelper>(),
                0,
                userGroupAlias,
                "Administrators",
                null)
            {
                Id = 10,
                Key = userGroupKey,
            };
        userGroupRepository
            .Setup(r => r.Get(It.IsAny<IQuery<IUserGroup>>()))
            .Returns(new[]
            {
                persistedUserGroup
            });
        var updatingUserGroup = new UserGroup(Mock.Of<IShortStringHelper>(), 0, persistedUserGroup.Alias + "updated",
            persistedUserGroup.Name + "updated", null)
        {
            Key = persistedUserGroup.Key,
            Id = persistedUserGroup.Id
        };

        var scopedNotificationPublisher = new Mock<IScopedNotificationPublisher>();
        scopedNotificationPublisher.Setup(p => p.PublishCancelableAsync(It.IsAny<ICancelableNotification>()))
            .ReturnsAsync(false);

        var scope = new Mock<ICoreScope>();
        scope.SetupGet(s => s.Notifications).Returns(scopedNotificationPublisher.Object);

        var query = new Mock<IQuery<IUserGroup>>();
        query.Setup(q => q.Where(It.IsAny<Expression<Func<IUserGroup, bool>>>())).Returns(query.Object);

        var provider = new Mock<ICoreScopeProvider>();
        provider.Setup(p => p.CreateQuery<IUserGroup>()).Returns(query.Object);
        provider.Setup(p => p.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher?>(),
            It.IsAny<IScopedNotificationPublisher?>(),
            It.IsAny<bool?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(scope.Object);

        var service = new UserGroupService(
            provider.Object,
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            userGroupRepository.Object,
            Mock.Of<IUserGroupPermissionService>(),
            Mock.Of<IEntityService>(),
            userService.Object,
            Mock.Of<ILogger<UserGroupService>>());

        // act
        var updateAttempt = await service.UpdateAsync(updatingUserGroup, actingUserKey);

        // assert
        Assert.AreEqual(status, updateAttempt.Status);
    }

    private IEnumerable<IReadOnlyUserGroup> CreateGroups(params string[] aliases)
        => aliases.Select(alias =>
        {
            var group = new Mock<IReadOnlyUserGroup>();
            group.SetupGet(g => g.Alias).Returns(alias);
            return group.Object;
        }).ToArray();

    private IUserGroupService SetupUserGroupServiceWithUserAndGetManyReturnsFourGroups(Guid userKey, string[] userGroupAliases)
    {
        var mockUser = SetupUserWithGroupAccess(userKey, userGroupAliases);

        var userService = SetupUserServiceWithGetUserByKey(userKey, mockUser);

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

    private Mock<IUser> SetupUserWithGroupAccess(Guid userKey, string[] userGroupAliases)
    {
        var user = new Mock<IUser>();
        user.SetupGet(u => u.Key).Returns(userKey);
        user.Setup(u => u.Groups).Returns(CreateGroups(userGroupAliases));
        return user;
    }

    private Mock<IUserService> SetupUserServiceWithGetUserByKey(Guid userKey, Mock<IUser> mockUser)
    {
        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetAsync(userKey)).Returns(Task.FromResult(mockUser.Object));
        return userService;
    }
}
