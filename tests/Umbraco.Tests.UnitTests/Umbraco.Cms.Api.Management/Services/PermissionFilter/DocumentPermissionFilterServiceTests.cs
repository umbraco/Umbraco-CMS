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
public class DocumentPermissionFilterServiceTests
{
    private class DocumentPermissionFilterServiceSetup
    {
        private readonly Guid _userKey = Guid.NewGuid();

        private DocumentPermissionFilterService? _sut;
        public DocumentPermissionFilterService Sut =>
            _sut ??= new DocumentPermissionFilterService(
                BackOfficeSecurityAccessor.Object,
                UserService.Object);

        public Mock<IBackOfficeSecurityAccessor> BackOfficeSecurityAccessor { get; } = new();

        public Mock<IUserService> UserService { get; } = new();

        public void SetupCurrentUser()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Key).Returns(_userKey);

            var backOfficeSecurityMock = new Mock<IBackOfficeSecurity>();
            backOfficeSecurityMock.Setup(b => b.CurrentUser).Returns(userMock.Object);

            BackOfficeSecurityAccessor
                .Setup(a => a.BackOfficeSecurity)
                .Returns(backOfficeSecurityMock.Object);
        }

        public void SetupGetDocumentPermissionsAsync(IEnumerable<NodePermissions> permissions)
        {
            var attempt = Attempt.SucceedWithStatus(UserOperationStatus.Success, permissions);
            UserService
                .Setup(s => s.GetDocumentPermissionsAsync(It.IsAny<Guid>(), It.IsAny<ISet<Guid>>()))
                .ReturnsAsync(attempt);
        }
    }

    private DocumentPermissionFilterServiceSetup _setup = null!;

    [SetUp]
    public void SetUp() => _setup = new DocumentPermissionFilterServiceSetup();

    [Test]
    public async Task FilterAsync_ReturnsAllEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(3);
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalItems) = await _setup.Sut.FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(3, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsync_FiltersEntities_WhenSomeAreDeniedBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(3);
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key), // No browse permission
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalItems) = await _setup.Sut.FilterAsync(entities, 100);

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
        var entities = CreateEntities(3);
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionBrowse.ActionLetter),
                // entities[1] has no permission entry - should be included
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalItems) = await _setup.Sut.FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(3, filteredEntities.Length);
        Assert.AreEqual(100, totalItems);
    }

    [Test]
    public async Task FilterAsync_FiltersAllEntities_WhenNoneHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(3);
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key))); // No browse permission

        // Act
        var (filteredEntities, totalItems) = await _setup.Sut.FilterAsync(entities, 100);

        // Assert
        Assert.AreEqual(0, filteredEntities.Length);
        Assert.AreEqual(97, totalItems);
    }

    [Test]
    public async Task FilterAsync_Siblings_ReturnsAllEntities_WhenAllHaveBrowsePermission()
    {
        // Arrange
        var entities = CreateEntities(5);
        var targetKey = entities[2].Key;
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            entities.Select(e => CreateNodePermissions(e.Key, ActionBrowse.ActionLetter)));

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await _setup.Sut.FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(5, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore);
        Assert.AreEqual(20, totalAfter);
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsTotalBefore_WhenEntityBeforeTargetIsFiltered()
    {
        // Arrange
        var entities = CreateEntities(5);
        var targetKey = entities[2].Key; // Index 2 is target, indices 0,1 are before
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key), // No browse - BEFORE target
                CreateNodePermissions(entities[1].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[4].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await _setup.Sut.FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(9, totalBefore); // Decremented by 1
        Assert.AreEqual(20, totalAfter); // Unchanged
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsTotalAfter_WhenEntityAfterTargetIsFiltered()
    {
        // Arrange
        var entities = CreateEntities(5);
        var targetKey = entities[2].Key; // Index 2 is target, indices 3,4 are after
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key), // No browse - AFTER target
                CreateNodePermissions(entities[4].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await _setup.Sut.FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore); // Unchanged
        Assert.AreEqual(19, totalAfter); // Decremented by 1
    }

    [Test]
    public async Task FilterAsync_Siblings_DecrementsBothTotals_WhenEntitiesBeforeAndAfterAreFiltered()
    {
        // Arrange
        var entities = CreateEntities(5);
        var targetKey = entities[2].Key;
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key), // No browse - BEFORE
                CreateNodePermissions(entities[1].Key), // No browse - BEFORE
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter), // Target
                CreateNodePermissions(entities[3].Key), // No browse - AFTER
                CreateNodePermissions(entities[4].Key), // No browse - AFTER
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await _setup.Sut.FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(1, filteredEntities.Length); // Only target remains
        Assert.AreEqual(8, totalBefore); // Decremented by 2
        Assert.AreEqual(18, totalAfter); // Decremented by 2
    }

    [Test]
    public async Task FilterAsync_Siblings_DoesNotAffectTotals_WhenTargetEntityIsFiltered()
    {
        // Arrange
        var entities = CreateEntities(5);
        var targetKey = entities[2].Key;
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[1].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[2].Key), // No browse - TARGET itself
                CreateNodePermissions(entities[3].Key, ActionBrowse.ActionLetter),
                CreateNodePermissions(entities[4].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalBefore, totalAfter) = await _setup.Sut.FilterAsync(targetKey, entities, 10, 20);

        // Assert
        Assert.AreEqual(4, filteredEntities.Length);
        Assert.AreEqual(10, totalBefore); // Unchanged - target is not before or after
        Assert.AreEqual(20, totalAfter); // Unchanged - target is not before or after
    }

    private static IEntitySlim[] CreateEntities(int count)
    {
        var entities = new IEntitySlim[count];
        for (var i = 0; i < count; i++)
        {
            var mock = new Mock<IEntitySlim>();
            mock.Setup(e => e.Key).Returns(Guid.NewGuid());
            mock.Setup(e => e.Id).Returns(i + 1);
            entities[i] = mock.Object;
        }

        return entities;
    }

    private static NodePermissions CreateNodePermissions(Guid nodeKey, params string[] permissions)
        => new()
        {
            NodeKey = nodeKey,
            Permissions = permissions.ToHashSet(),
        };
}
