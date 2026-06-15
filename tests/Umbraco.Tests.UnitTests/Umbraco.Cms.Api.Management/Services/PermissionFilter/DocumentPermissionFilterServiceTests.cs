using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.PermissionFilter;

[TestFixture]
public class DocumentPermissionFilterServiceTests
{
    private class DocumentPermissionFilterServiceSetup
    {
        private DocumentPermissionFilterService? _sut;

        public DocumentPermissionFilterService Sut =>
            _sut ??= new DocumentPermissionFilterService(
                BackOfficeSecurityAccessor.Object,
                ContentPermissionService.Object);

        public Mock<IBackOfficeSecurityAccessor> BackOfficeSecurityAccessor { get; } = new();

        public Mock<IContentPermissionService> ContentPermissionService { get; } = new();

        public void SetupCurrentUser()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Key).Returns(Guid.NewGuid());

            var backOfficeSecurityMock = new Mock<IBackOfficeSecurity>();
            backOfficeSecurityMock.Setup(b => b.CurrentUser).Returns(userMock.Object);

            BackOfficeSecurityAccessor
                .Setup(a => a.BackOfficeSecurity)
                .Returns(backOfficeSecurityMock.Object);
        }

        public void SetupGetDocumentPermissionsAsync(IEnumerable<NodePermissions> permissions)
        {
            ContentPermissionService
                .Setup(s => s.GetPermissionsAsync(It.IsAny<IUser>(), It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(permissions);
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
        Assert.That(filteredEntities.Length, Is.EqualTo(3));
        Assert.That(totalItems, Is.EqualTo(100));
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
        Assert.That(filteredEntities.Length, Is.EqualTo(2));
        Assert.That(totalItems, Is.EqualTo(99));
        Assert.That(filteredEntities.Any(e => e.Key == entities[0].Key), Is.True);
        Assert.That(filteredEntities.Any(e => e.Key == entities[1].Key), Is.False);
        Assert.That(filteredEntities.Any(e => e.Key == entities[2].Key), Is.True);
    }

    [Test]
    public async Task FilterAsync_FiltersEntities_WhenNoPermissionEntryExists()
    {
        // Arrange
        var entities = CreateEntities(3);
        _setup.SetupCurrentUser();
        _setup.SetupGetDocumentPermissionsAsync(
            [
                CreateNodePermissions(entities[0].Key, ActionBrowse.ActionLetter),
                // entities[1] has no permission entry - should be denied (fail-closed)
                CreateNodePermissions(entities[2].Key, ActionBrowse.ActionLetter),
            ]);

        // Act
        var (filteredEntities, totalItems) = await _setup.Sut.FilterAsync(entities, 100);

        // Assert
        Assert.That(filteredEntities.Length, Is.EqualTo(2));
        Assert.That(totalItems, Is.EqualTo(99));
        Assert.That(filteredEntities.Any(e => e.Key == entities[0].Key), Is.True);
        Assert.That(filteredEntities.Any(e => e.Key == entities[1].Key), Is.False);
        Assert.That(filteredEntities.Any(e => e.Key == entities[2].Key), Is.True);
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
        Assert.That(filteredEntities.Length, Is.EqualTo(0));
        Assert.That(totalItems, Is.EqualTo(97));
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
        Assert.That(filteredEntities.Length, Is.EqualTo(5));
        Assert.That(totalBefore, Is.EqualTo(10));
        Assert.That(totalAfter, Is.EqualTo(20));
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
        Assert.That(filteredEntities.Length, Is.EqualTo(4));
        Assert.That(totalBefore, Is.EqualTo(9)); // Decremented by 1
        Assert.That(totalAfter, Is.EqualTo(20)); // Unchanged
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
        Assert.That(filteredEntities.Length, Is.EqualTo(4));
        Assert.That(totalBefore, Is.EqualTo(10)); // Unchanged
        Assert.That(totalAfter, Is.EqualTo(19)); // Decremented by 1
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
        Assert.That(filteredEntities.Length, Is.EqualTo(1)); // Only target remains
        Assert.That(totalBefore, Is.EqualTo(8)); // Decremented by 2
        Assert.That(totalAfter, Is.EqualTo(18)); // Decremented by 2
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
        Assert.That(filteredEntities.Length, Is.EqualTo(4));
        Assert.That(totalBefore, Is.EqualTo(10)); // Unchanged - target is not before or after
        Assert.That(totalAfter, Is.EqualTo(20)); // Unchanged - target is not before or after
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
