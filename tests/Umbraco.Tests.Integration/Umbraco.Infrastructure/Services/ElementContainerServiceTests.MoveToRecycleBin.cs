using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Move_Empty_Container_From_Root_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(1, GetAtRoot().Length);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(0, GetAtRoot().Length);

        await AssertContainerIsInRecycleBin(rootContainerKey);
    }

    [Test]
    public async Task Can_Move_Empty_Container_From_Another_Container_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        await AssertContainerIsInRecycleBin(childContainerKey);
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Root_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        var grandchildContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(grandchildContainerKey).Length);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(0, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey, true).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey, true).Length);
        Assert.AreEqual(0, GetFolderChildren(grandchildContainerKey, true).Length);

        await AssertContainerIsInRecycleBin(rootContainerKey);
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Another_Container_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        var grandchildContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey);

        var greatGrandchildContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey, true).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey, true).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey, true).Length);

        await AssertContainerIsInRecycleBin(childContainerKey);
    }

    [TestCase(true)]
    [TestCase(false)]
    [LongRunning]
    public async Task Can_Move_Container_With_Descendant_Containers_And_Lots_Of_Elements_To_Recycle_Bin(bool createChildContainerAtRoot)
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(createChildContainerAtRoot);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(setup.ChildContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        await AssertContainerIsInRecycleBin(setup.ChildContainerKey);

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.NotNull(childContainer);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(2, grandchildContainer.Level);
        Assert.IsTrue(grandchildContainer.Trashed);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(setup.RootContainerKey).Length);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey, true);
        Assert.AreEqual(setup.ChildContainerItems, grandchildren.Length);

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey, true);
        Assert.AreEqual(setup.GrandchildContainerItems, greatGrandchildren.Length);

        foreach (var element in grandchildren)
        {
            Assert.AreEqual(childContainer.Id, element.ParentId);
            Assert.AreEqual($"{childContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(2, element.Level);
            Assert.IsTrue(element.Trashed);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.AreEqual(grandchildContainer.Id, element.ParentId);
            Assert.AreEqual($"{grandchildContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(3, element.Level);
            Assert.IsTrue(element.Trashed);
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Move_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Root_To_Recycle_Bin()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        await AssertContainerIsInRecycleBin(setup.RootContainerKey);

        var rootContainer = await ElementContainerService.GetAsync(setup.RootContainerKey);
        Assert.NotNull(rootContainer);

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);
        Assert.IsTrue(childContainer.Trashed);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);
        Assert.IsTrue(grandchildContainer.Trashed);

        Assert.AreEqual(0, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(setup.RootContainerKey, true).Length);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey, true);
        Assert.AreEqual(setup.ChildContainerItems, grandchildren.Length);

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey, true);
        Assert.AreEqual(setup.GrandchildContainerItems, greatGrandchildren.Length);

        foreach (var element in grandchildren)
        {
            Assert.AreEqual(childContainer.Id, element.ParentId);
            Assert.AreEqual($"{childContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(3, element.Level);
            Assert.IsTrue(element.Trashed);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.AreEqual(grandchildContainer.Id, element.ParentId);
            Assert.AreEqual($"{grandchildContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(4, element.Level);
            Assert.IsTrue(element.Trashed);
        }
    }

    [Test]
    public async Task Container_Move_To_Recycle_Bin_Events_Are_Fired()
    {
        var movingWasCalled = false;
        var movedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(container);

        try
        {
            EntityContainerNotificationHandler.MovingContainerToRecycleBin = notification =>
            {
                movingWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(containerKey, moveInfo.Entity.Key);
                Assert.AreEqual(container.Path, moveInfo.OriginalPath);
            };

            EntityContainerNotificationHandler.MovedContainerToRecycleBin = notification =>
            {
                movedWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(containerKey, moveInfo.Entity.Key);
                Assert.AreEqual(container.Path, moveInfo.OriginalPath);
            };

            var result = await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(movingWasCalled);
            Assert.IsTrue(movedWasCalled);

            Assert.AreEqual(0, GetAtRoot().Length);
        }
        finally
        {
            EntityContainerNotificationHandler.MovingContainerToRecycleBin = null;
            EntityContainerNotificationHandler.MovedContainerToRecycleBin = null;
        }
    }

    [Test]
    public async Task Container_Moving_To_Recycle_Bin_Event_Can_Be_Cancelled()
    {
        var movingWasCalled = false;
        var movedWasCalled = false;

        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.MovingContainerToRecycleBin = notification =>
            {
                movingWasCalled = true;
                notification.Cancel = true;
            };

            EntityContainerNotificationHandler.MovedContainerToRecycleBin = _ =>
            {
                movedWasCalled = true;
            };

            var result = await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(movingWasCalled);
            Assert.IsFalse(movedWasCalled);

            Assert.AreEqual(1, GetAtRoot().Length);
        }
        finally
        {
            EntityContainerNotificationHandler.MovingContainerToRecycleBin = null;
            EntityContainerNotificationHandler.MovedContainerToRecycleBin = null;
        }
    }

    [Test]
    public async Task Cannot_Move_Container_To_Container_In_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var trashedContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(trashedContainerKey, "Trashed Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, trashedContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InTrash, moveResult.Status);
        });

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.IsNotNull(rootContainer);
        Assert.IsFalse(rootContainer.Trashed);

        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(trashedContainerKey, true).Length);
    }

    [Test]
    public async Task Cannot_Move_Container_In_Recycle_Bin_To_Other_Container_In_Recycle_Bin()
    {
        var firstContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(firstContainerKey, "First Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(firstContainerKey, Constants.Security.SuperUserKey);

        var secondContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(secondContainerKey, "Second Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(secondContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementContainerService.MoveAsync(firstContainerKey, secondContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InTrash, moveResult.Status);
        });

        var firstContainer = await ElementContainerService.GetAsync(firstContainerKey);
        Assert.IsNotNull(firstContainer);
        Assert.IsTrue(firstContainer.Trashed);
        Assert.AreEqual(Constants.System.RecycleBinElement, firstContainer.ParentId);
    }
}
