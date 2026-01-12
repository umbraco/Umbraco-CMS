using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Move_Empty_Container_From_Root_To_Another_Container()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var otherRootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(otherRootContainerKey, "Other Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", otherRootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);

        Assert.AreEqual(0, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(otherRootContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.NotNull(rootContainer);
        var otherRootContainer = await ElementContainerService.GetAsync(otherRootContainerKey);
        Assert.NotNull(otherRootContainer);

        Assert.AreEqual(childContainer.Id, rootContainer.ParentId);
        Assert.AreEqual($"{Constants.System.Root},{otherRootContainer.Id},{childContainer.Id},{rootContainer.Id}", rootContainer.Path);
        Assert.AreEqual(childContainer.Level + 1, rootContainer.Level);
    }

    [Test]
    public async Task Can_Move_Empty_Container_From_Another_Container_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(Constants.System.Root, childContainer.ParentId);
        Assert.AreEqual($"{Constants.System.Root},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(1, childContainer.Level);
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Another_Container_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(rootContainer);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(4, greatGrandchildContainer.Level);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(Constants.System.Root, childContainer.ParentId);
        Assert.AreEqual($"{Constants.System.Root},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(1, childContainer.Level);

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(2, grandchildContainer.Level);

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(3, greatGrandchildContainer.Level);
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Root_To_Another_Container()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(rootContainer);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);
        Assert.AreEqual(Constants.System.Root, childContainer.ParentId);
        Assert.AreEqual($"{Constants.System.Root},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(1, childContainer.Level);

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(2, grandchildContainer.Level);

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(3, greatGrandchildContainer.Level);

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(4, greatGrandchildContainer.Level);
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_One_Container_To_Another_Container()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(rootContainer);

        var otherRootContainerKey = Guid.NewGuid();
        var otherRootContainer = (await ElementContainerService.CreateAsync(otherRootContainerKey, "Other Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(otherRootContainer);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(4, greatGrandchildContainer.Level);

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(otherRootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, otherRootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(otherRootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(grandchildContainerKey).Length);
        Assert.AreEqual(0, GetFolderChildren(greatGrandchildContainerKey).Length);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(otherRootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{otherRootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.NotNull(greatGrandchildContainer);
        Assert.AreEqual(grandchildContainer.Id, greatGrandchildContainer.ParentId);
        Assert.AreEqual($"{grandchildContainer.Path},{greatGrandchildContainer.Id}", greatGrandchildContainer.Path);
        Assert.AreEqual(4, greatGrandchildContainer.Level);
    }

    [Test]
    [LongRunning]
    public async Task Can_Move_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Root_To_Another_Container()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(true);

        var moveResult = await ElementContainerService.MoveAsync(setup.ChildContainerKey, setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        var rootContainer = await ElementContainerService.GetAsync(setup.RootContainerKey);
        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.NotNull(rootContainer);
        Assert.NotNull(childContainer);
        Assert.AreEqual(rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{rootContainer.Path},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(2, childContainer.Level);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(3, grandchildContainer.Level);

        Assert.AreEqual(1, GetAtRoot().Length);
        Assert.AreEqual(1, GetFolderChildren(setup.RootContainerKey).Length);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey);
        Assert.AreEqual(setup.ChildContainerItems, grandchildren.Length);

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey);
        Assert.AreEqual(setup.GrandchildContainerItems, greatGrandchildren.Length);

        foreach (var element in grandchildren)
        {
            Assert.AreEqual(childContainer.Id, element.ParentId);
            Assert.AreEqual($"{childContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(3, element.Level);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.AreEqual(grandchildContainer.Id, element.ParentId);
            Assert.AreEqual($"{grandchildContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(4, element.Level);
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Move_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_A_Container_To_Root()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);

        var moveResult = await ElementContainerService.MoveAsync(setup.ChildContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.NotNull(childContainer);
        Assert.AreEqual(Constants.System.Root, childContainer.ParentId);
        Assert.AreEqual($"{Constants.System.Root},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(1, childContainer.Level);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.NotNull(grandchildContainer);
        Assert.AreEqual(childContainer.Id, grandchildContainer.ParentId);
        Assert.AreEqual($"{childContainer.Path},{grandchildContainer.Id}", grandchildContainer.Path);
        Assert.AreEqual(2, grandchildContainer.Level);

        Assert.AreEqual(2, GetAtRoot().Length);
        Assert.AreEqual(0, GetFolderChildren(setup.RootContainerKey).Length);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey);
        Assert.AreEqual(setup.ChildContainerItems, grandchildren.Length);

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey);
        Assert.AreEqual(setup.GrandchildContainerItems, greatGrandchildren.Length);

        foreach (var element in grandchildren)
        {
            Assert.AreEqual(childContainer.Id, element.ParentId);
            Assert.AreEqual($"{childContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(2, element.Level);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.AreEqual(grandchildContainer.Id, element.ParentId);
            Assert.AreEqual($"{grandchildContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(3, element.Level);
        }
    }

    [Test]
    public async Task Can_Move_Container_With_Descendants_From_Recycle_Bin_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.IsNotNull(childElement);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.IsNotNull(grandchildElement);

        Assert.AreEqual(2, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);
        await AssertContainerIsInRecycleBin(rootContainerKey);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.IsNotNull(childContainer);
        Assert.IsTrue(childContainer.Trashed);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.IsNotNull(childElement);
        Assert.IsTrue(childElement.Trashed);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.IsNotNull(grandchildElement);
        Assert.IsTrue(grandchildElement.Trashed);

        Assert.AreEqual(2, GetFolderChildren(rootContainerKey, true).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey, true).Length);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.IsNotNull(rootContainer);
        Assert.IsFalse(rootContainer.Trashed);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.IsNotNull(childContainer);
        Assert.IsFalse(childContainer.Trashed);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.IsNotNull(childElement);
        Assert.IsFalse(childElement.Trashed);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.IsNotNull(grandchildElement);
        Assert.IsFalse(grandchildElement.Trashed);

        Assert.AreEqual(2, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);
    }

    [Test]
    public async Task Restoring_Trashed_Container_Performs_Explicit_Unpublish_Of_All_Descendant_Elements()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.IsNotNull(childElement);

        var publishResult = await ElementPublishingService.PublishAsync(
            childElement.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.IsNotNull(grandchildElement);

        publishResult = await ElementPublishingService.PublishAsync(
            grandchildElement.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);
        await AssertContainerIsInRecycleBin(rootContainerKey);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.NotNull(childElement);
        Assert.IsTrue(childElement.Published);
        Assert.IsTrue(childElement.Trashed);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.NotNull(grandchildElement);
        Assert.IsTrue(grandchildElement.Published);
        Assert.IsTrue(grandchildElement.Trashed);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, moveResult.Status);
        });

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.NotNull(childElement);
        Assert.IsFalse(childElement.Published);
        Assert.IsFalse(childElement.Trashed);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.NotNull(grandchildElement);
        Assert.IsFalse(grandchildElement.Published);
        Assert.IsFalse(grandchildElement.Trashed);
    }

    [Test]
    public async Task Container_Move_Events_Are_Fired()
    {
        var movingWasCalled = false;
        var movedWasCalled = false;

        var firstContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(firstContainerKey, "First Container", null, Constants.Security.SuperUserKey);

        var secondContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(secondContainerKey, "Second Container", null, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.MovingContainer = notification =>
            {
                movingWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(secondContainerKey, moveInfo.Entity.Key);
                Assert.AreEqual(firstContainerKey, moveInfo.NewParentKey);
            };

            EntityContainerNotificationHandler.MovedContainer = notification =>
            {
                movedWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(secondContainerKey, moveInfo.Entity.Key);
                Assert.AreEqual(firstContainerKey, moveInfo.NewParentKey);
            };

            var result = await ElementContainerService.MoveAsync(secondContainerKey, firstContainerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(movingWasCalled);
            Assert.IsTrue(movedWasCalled);

            Assert.AreEqual(1, GetFolderChildren(firstContainerKey).Length);
        }
        finally
        {
            EntityContainerNotificationHandler.MovingContainer = null;
            EntityContainerNotificationHandler.MovedContainer = null;
        }
    }

    [Test]
    public async Task Container_Moving_Event_Can_Be_Cancelled()
    {
        var movingWasCalled = false;
        var movedWasCalled = false;

        var firstContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(firstContainerKey, "First Container", null, Constants.Security.SuperUserKey);

        var secondContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(secondContainerKey, "Second Container", null, Constants.Security.SuperUserKey);

        try
        {
            EntityContainerNotificationHandler.MovingContainer = notification =>
            {
                movingWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(secondContainerKey, moveInfo.Entity.Key);
                Assert.AreEqual(firstContainerKey, moveInfo.NewParentKey);

                notification.Cancel = true;
            };

            EntityContainerNotificationHandler.MovedContainer = _ =>
            {
                movedWasCalled = true;
            };

            var result = await ElementContainerService.MoveAsync(secondContainerKey, firstContainerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(movingWasCalled);
            Assert.IsFalse(movedWasCalled);

            Assert.AreEqual(0, GetFolderChildren(firstContainerKey).Length);
        }
        finally
        {
            EntityContainerNotificationHandler.MovingContainer = null;
            EntityContainerNotificationHandler.MovedContainer = null;
        }
    }

    [Test]
    public async Task Cannot_Move_Container_To_Self()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetFolderChildren(rootContainerKey).Length);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InvalidParent, moveResult.Status);
        });
    }

    [Test]
    public async Task Cannot_Move_Container_To_Child_Of_Self()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InvalidParent, moveResult.Status);
        });
    }

    [Test]
    public async Task Cannot_Move_Container_To_Descendant_Of_Self()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        var grandchildContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, grandchildContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InvalidParent, moveResult.Status);
        });
    }
}
