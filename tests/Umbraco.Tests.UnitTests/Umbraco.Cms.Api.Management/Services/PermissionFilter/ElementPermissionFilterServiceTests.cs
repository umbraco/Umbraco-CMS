using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.PermissionFilter;

[TestFixture]
public class ElementPermissionFilterServiceTests
{
    private readonly Mock<IBackOfficeSecurityAccessor> _backOfficeSecurityAccessor = new(MockBehavior.Strict);
    private readonly Mock<IUserService> _userService = new(MockBehavior.Strict);

    private ElementPermissionFilterService ElementPermissionFilterService
        => new(_backOfficeSecurityAccessor.Object, _userService.Object);

    [SetUp]
    public void SetUp() => SetupCurrentUser();

    [Test]
    public async Task FilterAsync_ReturnsAllEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateElementEntities(3);
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionElementBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(3, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsync_FiltersEntities_WhenSomeAreDeniedBrowsePermission()
    {
        // Arrange
        var entities = CreateElementEntities(3);
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key), // No browse permission
                CreateNodePermissions(entities[2].Key, ActionElementBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(2, filteredEntities.Length);
        Assert.AreEqual(99, totalItems);
        Assert.IsTrue(filteredEntities.Any(e => e.Key == entities[0].Key));
        Assert.IsFalse(filteredEntities.Any(e => e.Key == entities[1].Key));
        Assert.IsTrue(filteredEntities.Any(e => e.Key == entities[2].Key));
    }

    [Test]
    public async Task FilterAsync_IncludesEntities_WhenNoPermissionEntryExists()
    {
        // Arrange
        var entities = CreateElementEntities(3);
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionElementBrowse.ActionLetter),
                // entities[1] has no permission entry - should be included
                CreateNodePermissions(entities[2].Key, ActionElementBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(3, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsync_FiltersAllEntities_WhenNoneHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateElementEntities(3);
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key))); // No browse permission

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(0, filteredEntities.Length);
        Assert.AreEqual(97, totalItems);
    }

    [Test]
    public async Task FilterAsync_Siblings_ReturnsAllEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateElementEntities(5);
        var targetKey = entities[2].Key;
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionElementBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await ElementPermissionFilterService
            .FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(5, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore);
        Assert.AreEqual(20, totalAfter);
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsTotalBefore_WhenEntityBeforeTargetIsFiltered()
    {
        // Arrange
        var entities = CreateElementEntities(5);
        var targetKey = entities[2].Key; // Index 2 is target, indices 0,1 are before
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key), // No browse - BEFORE target
                CreateNodePermissions(entities[1].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key, ActionElementBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[4].Key, ActionElementBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await ElementPermissionFilterService
            .FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(9, totalBefore); // Decremented by 1
        Assert.AreEqual(20, totalAfter); // Unchanged
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsTotalAfter_WhenEntityAfterTargetIsFiltered()
    {
        // Arrange
        var entities = CreateElementEntities(5);
        var targetKey = entities[2].Key; // Index 2 is target, indices 3,4 are after
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key, ActionElementBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key), // No browse - AFTER target
                CreateNodePermissions(entities[4].Key, ActionElementBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await ElementPermissionFilterService
            .FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore); // Unchanged
        Assert.AreEqual(19, totalAfter); // Decremented by 1
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsBothTotals_WhenEntitiesBeforeAndAfterAreFiltered()
    {
        // Arrange
        var entities = CreateElementEntities(5);
        var targetKey = entities[2].Key;
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key), // No browse - BEFORE
                CreateNodePermissions(entities[1].Key), // No browse - BEFORE
                CreateNodePermissions(entities[2].Key, ActionElementBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key), // No browse - AFTER
                CreateNodePermissions(entities[4].Key), // No browse - AFTER
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await ElementPermissionFilterService
            .FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(1, filteredEntities.Length); // Only target remains
        Assert.AreEqual(8, totalBefore); // Decremented by 2
        Assert.AreEqual(18, totalAfter); // Decremented by 2
    }

    [Test]
    public async Task FilterAsync_Siblings_DoesNotAffectTotals_WhenTargetEntityIsFiltered()
    {
        // Arrange
        var entities = CreateElementEntities(5);
        var targetKey = entities[2].Key;
        SetupGetElementPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key), // No browse - TARGET itself
                CreateNodePermissions(entities[3].Key, ActionElementBrowse.ActionLetter),
                CreateNodePermissions(entities[4].Key, ActionElementBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await ElementPermissionFilterService
            .FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore); // Unchanged - target is not before or after
        Assert.AreEqual(20, totalAfter); // Unchanged - target is not before or after
    }

    [Test]
    public async Task FilterAsync_ReturnsAllContainerEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateElementContainerEntities(3);
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionElementFolderBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(3, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsyncMixedEntities_ReturnsAllEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(10, i => i % 2 == 0 ? Constants.ObjectTypes.Element : Constants.ObjectTypes.ElementContainer);
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionElementBrowse.ActionLetter, ActionElementFolderBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(10, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsyncMixedEntities_ReturnsOnlyAllowedEntities_WhenSomeHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(10, i => i % 2 == 0 ? Constants.ObjectTypes.Element : Constants.ObjectTypes.ElementContainer);
        var count = 0;
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, count++ % 5 == 0 ? [] : [ActionElementBrowse.ActionLetter, ActionElementFolderBrowse.ActionLetter])));

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(8, filteredEntities.Length); // Decremented by 2
        Assert.AreEqual(98, totalItems); // Decremented by 2
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task FilterAsyncEntities_ReturnsNoEntities_WhenNoneHaveBrowsePermission(bool isContainer)
    {
        // Arrange
        var entities = CreateEntities(5, _ => isContainer ? Constants.ObjectTypes.ElementContainer : Constants.ObjectTypes.Element);
        SetupGetElementPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, isContainer ? ActionElementBrowse.ActionLetter : ActionElementFolderBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalItems) = await ElementPermissionFilterService
            .FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(0, filteredEntities.Length);
        Assert.AreEqual(95, totalItems); // Decremented by 5
    }

    private static IEntitySlim[] CreateElementEntities(int count)
        => CreateEntities(count, _ => Constants.ObjectTypes.Element);

    private static IEntitySlim[] CreateElementContainerEntities(int count)
        => CreateEntities(count, _ => Constants.ObjectTypes.ElementContainer);

    private static IEntitySlim[] CreateEntities(int count, Func<int, Guid> objectTypeForIndex)
    {
        var entities = new IEntitySlim[count];
        for (var i = 0; i < count; i++)
        {
            var mock = new Mock<IEntitySlim>();
            mock.Setup(e => e.Key).Returns(Guid.NewGuid());
            mock.Setup(e => e.Id).Returns(i + 1);
            mock.Setup(e => e.NodeObjectType).Returns(objectTypeForIndex(i));
            entities[i] = mock.Object;
        }

        return entities;
    }

    private void SetupCurrentUser()
    {
        var userMock = new Mock<IUser>();
        userMock.Setup(u => u.Key).Returns(Guid.NewGuid());

        var backOfficeSecurityMock = new Mock<IBackOfficeSecurity>();
        backOfficeSecurityMock.Setup(b => b.CurrentUser).Returns(userMock.Object);

        _backOfficeSecurityAccessor
            .Setup(a => a.BackOfficeSecurity)
            .Returns(backOfficeSecurityMock.Object);
    }

    private void SetupGetElementPermissionsAsync(IEnumerable<NodePermissions> permissions)
    {
        var attempt = Attempt.SucceedWithStatus(UserOperationStatus.Success, permissions);
        _userService
            .Setup(s => s.GetElementPermissionsAsync(It.IsAny<Guid>(), It.IsAny<ISet<Guid>>()))
            .ReturnsAsync(attempt);
    }

    private static NodePermissions CreateNodePermissions(Guid nodeKey, params string[] permissions)
        => new()
        {
            NodeKey = nodeKey,
            Permissions = permissions.ToHashSet(),
        };
}
