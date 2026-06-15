using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Delete_Empty_Container_From_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);

        await AssertContainerIsInRecycleBin(rootContainerKey);

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        // verify that the deletion happened
        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.That(rootContainer, Is.Null);
    }

    [Test]
    [LongRunning]
    public async Task Can_Delete_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Recycle_Bin()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.Not.EqualTo(0));

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task Cannot_Delete_Untrashed_Container_From_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Status, Is.EqualTo(EntityContainerOperationStatus.NotInTrash));
        });

        // verify that the deletion did not happen
        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.That(rootContainer, Is.Not.Null);
        Assert.That(rootContainer.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Container_Delete_From_Recycle_Bin_Events_Are_Fired()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                Assert.That(notification.DeletedEntities.Single().Key, Is.EqualTo(containerKey));
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedWasCalled = true;
                Assert.That(notification.DeletedEntities.Single().Key, Is.EqualTo(containerKey));
            };

            var result = await ElementContainerService.DeleteFromRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
            Assert.That(result.Success, Is.True);
            Assert.That(deletingWasCalled, Is.True);
            Assert.That(deletedWasCalled, Is.True);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        Assert.That(GetAtRoot(), Is.Empty);
        Assert.That(await ElementContainerService.GetAsync(containerKey), Is.Null);
    }

    [Test]
    public async Task Container_Delete_From_Recycle_Bin_Event_Can_Be_Cancelled()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                notification.Cancel = true;
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedWasCalled = true;
            };

            var result = await ElementContainerService.DeleteFromRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.CancelledByNotification));
            Assert.That(result.Success, Is.False);
            Assert.That(deletingWasCalled, Is.True);
            Assert.That(deletedWasCalled, Is.False);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        var entityContainer = await ElementContainerService.GetAsync(containerKey);
        Assert.That(entityContainer, Is.Not.Null);
        Assert.That(entityContainer.Trashed, Is.True);
    }

    [Test]
    public async Task Deleted_Notifications_Are_Fired_For_Descendant_Items()
    {
        var deletedElementKeys = new List<Guid>();
        var deletedContainerKeys = new List<Guid>();

        var elementType = await CreateElementType();

        // Create a container structure: rootContainer -> childContainer -> elements
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        // Create elements in the child container
        var element1 = await CreateElement(elementType.Key, childContainerKey);
        var element2 = await CreateElement(elementType.Key, childContainerKey);

        // Move to recycle bin
        await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);

        try
        {
            ElementNotificationHandler.DeletedElement = notification =>
            {
                deletedElementKeys.Add(notification.DeletedEntities.Single().Key);
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedContainerKeys.Add(notification.DeletedEntities.Single().Key);
            };

            var result = await ElementContainerService.DeleteFromRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

            // Verify notifications were fired for descendant elements
            Assert.That(deletedElementKeys, Has.Count.EqualTo(2));
            Assert.That(deletedElementKeys, Does.Contain(element1.Key));
            Assert.That(deletedElementKeys, Does.Contain(element2.Key));

            // Verify notifications were fired for descendant containers (child + root)
            Assert.That(deletedContainerKeys, Has.Count.EqualTo(2));
            Assert.That(deletedContainerKeys, Does.Contain(childContainerKey));
            Assert.That(deletedContainerKeys, Does.Contain(rootContainerKey));
        }
        finally
        {
            ElementNotificationHandler.DeletedElement = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }
    }

    [Test]
    public async Task Can_Delete_Container_Set_As_User_Start_Node_From_Recycle_Bin()
    {
        var userService = GetRequiredService<IUserService>();

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Start Node Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        var user = new UserBuilder()
            .WithName("StartNodeUser")
            .WithStartElementId(container!.Id)
            .Build();
        userService.Save(user);

        await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        await AssertContainerIsInRecycleBin(containerKey);

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(await ElementContainerService.GetAsync(containerKey), Is.Null);

        // The user's start node reference should have been cleared.
        var updatedUser = userService.GetUserById(user.Id);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.StartElementIds?.Contains(container.Id) ?? false, Is.False);
    }

    [Test]
    public async Task Can_Delete_Container_Set_As_User_Group_Start_Node_From_Recycle_Bin()
    {
        var userGroupService = GetRequiredService<IUserGroupService>();

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Start Node Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithStartElementId(container!.Id)
            .Build();
        var groupResult = await userGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.That(groupResult.Success, Is.True);

        await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        await AssertContainerIsInRecycleBin(containerKey);

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(await ElementContainerService.GetAsync(containerKey), Is.Null);

        // The user group's start node reference should have been cleared.
        var updatedGroup = await userGroupService.GetAsync(userGroup.Key);
        Assert.That(updatedGroup, Is.Not.Null);
        Assert.That(updatedGroup!.StartElementId, Is.Null);
    }
}
