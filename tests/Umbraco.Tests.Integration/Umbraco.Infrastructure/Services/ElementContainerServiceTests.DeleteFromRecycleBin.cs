using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;

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
            Assert.IsTrue(deleteResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, deleteResult.Status);
        });

        // verify that the deletion happened
        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.IsNull(rootContainer);
    }

    [Test]
    [LongRunning]
    public async Task Can_Delete_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Recycle_Bin()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreNotEqual(0, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(deleteResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, deleteResult.Status);
        });

        Assert.AreEqual(0, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());
    }

    [Test]
    public async Task Cannot_Delete_Untrashed_Container_From_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        Assert.AreEqual(1, GetAtRoot().Length);

        var deleteResult = await ElementContainerService.DeleteFromRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(deleteResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.NotInTrash, deleteResult.Status);
        });

        // verify that the deletion did not happen
        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.IsNotNull(rootContainer);
        Assert.AreEqual(Constants.System.Root, rootContainer.ParentId);
    }

    [Test]
    public async Task Container_Delete_From_Recycle_Bin_Events_Are_Fired()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(container);

        await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                Assert.AreEqual(containerKey, notification.DeletedEntities.Single().Key);
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedWasCalled = true;
                Assert.AreEqual(containerKey, notification.DeletedEntities.Single().Key);
            };

            var result = await ElementContainerService.DeleteFromRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(deletingWasCalled);
            Assert.IsTrue(deletedWasCalled);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        Assert.AreEqual(0, GetAtRoot().Length);
        Assert.IsNull(await ElementContainerService.GetAsync(containerKey));
    }

    [Test]
    public async Task Container_Delete_From_Recycle_Bin_Event_Can_Be_Cancelled()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(container);

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

            Assert.AreEqual(EntityContainerOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(deletingWasCalled);
            Assert.IsFalse(deletedWasCalled);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        var entityContainer = await ElementContainerService.GetAsync(containerKey);
        Assert.IsNotNull(entityContainer);
        Assert.IsTrue(entityContainer.Trashed);
    }
}
