using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Move_Empty_Container_From_Root_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Is.Empty);

        await AssertContainerIsInRecycleBin(rootContainerKey);
    }

    [Test]
    public async Task Can_Move_Empty_Container_From_Another_Container_To_Recycle_Bin()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey);

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

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

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Is.Empty);
        Assert.That(GetFolderChildren(rootContainerKey, true), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey, true), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey, true), Is.Empty);

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

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(childContainerKey, true), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey, true), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey, true), Is.Empty);

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
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        await AssertContainerIsInRecycleBin(setup.ChildContainerKey);

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.That(childContainer, Is.Not.Null);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(2));
        Assert.That(grandchildContainer.Trashed, Is.True);

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(setup.RootContainerKey), Is.Empty);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey, true);
        Assert.That(grandchildren, Has.Length.EqualTo(setup.ChildContainerItems));

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey, true);
        Assert.That(greatGrandchildren, Has.Length.EqualTo(setup.GrandchildContainerItems));

        foreach (var element in grandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(childContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{childContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(2));
            Assert.That(element.Trashed, Is.True);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(grandchildContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{grandchildContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(3));
            Assert.That(element.Trashed, Is.True);
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
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        await AssertContainerIsInRecycleBin(setup.RootContainerKey);

        var rootContainer = await ElementContainerService.GetAsync(setup.RootContainerKey);
        Assert.That(rootContainer, Is.Not.Null);

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));
        Assert.That(childContainer.Trashed, Is.True);

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));
        Assert.That(grandchildContainer.Trashed, Is.True);

        Assert.That(GetAtRoot(), Is.Empty);
        Assert.That(GetFolderChildren(setup.RootContainerKey, true), Has.Length.EqualTo(1));

        var grandchildren = GetFolderChildren(setup.ChildContainerKey, true);
        Assert.That(grandchildren, Has.Length.EqualTo(setup.ChildContainerItems));

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey, true);
        Assert.That(greatGrandchildren, Has.Length.EqualTo(setup.GrandchildContainerItems));

        foreach (var element in grandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(childContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{childContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(3));
            Assert.That(element.Trashed, Is.True);
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(grandchildContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{grandchildContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(4));
            Assert.That(element.Trashed, Is.True);
        }
    }

    [Test]
    public async Task Container_Move_To_Recycle_Bin_Events_Are_Fired()
    {
        var movingWasCalled = false;
        var movedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        try
        {
            EntityContainerNotificationHandler.MovingContainerToRecycleBin = notification =>
            {
                movingWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.That(moveInfo.Entity.Key, Is.EqualTo(containerKey));
                Assert.That(moveInfo.OriginalPath, Is.EqualTo(container.Path));
            };

            EntityContainerNotificationHandler.MovedContainerToRecycleBin = notification =>
            {
                movedWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.That(moveInfo.Entity.Key, Is.EqualTo(containerKey));
                Assert.That(moveInfo.OriginalPath, Is.EqualTo(container.Path));
            };

            var result = await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
            Assert.That(result.Success, Is.True);
            Assert.That(movingWasCalled, Is.True);
            Assert.That(movedWasCalled, Is.True);

            Assert.That(GetAtRoot(), Is.Empty);
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

            Assert.That(result.Result, Is.EqualTo(EntityContainerOperationStatus.CancelledByNotification));
            Assert.That(result.Success, Is.False);
            Assert.That(movingWasCalled, Is.True);
            Assert.That(movedWasCalled, Is.False);

            Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
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
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.InTrash));
        });

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.That(rootContainer, Is.Not.Null);
        Assert.That(rootContainer.Trashed, Is.False);

        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(trashedContainerKey, true), Is.Empty);
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
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.InTrash));
        });

        var firstContainer = await ElementContainerService.GetAsync(firstContainerKey);
        Assert.That(firstContainer, Is.Not.Null);
        Assert.That(firstContainer.Trashed, Is.True);
        Assert.That(firstContainer.ParentId, Is.EqualTo(Constants.System.RecycleBinElement));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableUnpublishWhenReferenced))]
    public async Task Cannot_Move_Container_To_Recycle_Bin_When_Descendant_Is_Referenced_And_Configured_To_Disable_When_Referenced()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var elementType = await CreateElementType();
        var referencingElement = await CreateElement(elementType.Key);
        var referencedElement = await CreateElement(elementType.Key, containerKey);

        // Set up a relation where referencingElement references referencedElement (inside the container).
        RelationService.Relate(
            referencingElement.Id,
            referencedElement.Id,
            Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.HasReferencedDescendants));
        });

        // Verify the container was not moved
        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.That(container.Trashed, Is.False);
    }

    [Test]
    public async Task Can_Move_Container_To_Recycle_Bin_When_Descendant_Is_Referenced_But_Not_Configured_To_Disable()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var elementType = await CreateElementType();
        var referencingElement = await CreateElement(elementType.Key);
        var referencedElement = await CreateElement(elementType.Key, containerKey);

        // Set up a relation where referencingElement references referencedElement (inside the container).
        RelationService.Relate(
            referencingElement.Id,
            referencedElement.Id,
            Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        // Without the DisableUnpublishWhenReferenced setting, the move should succeed.
        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        await AssertContainerIsInRecycleBin(containerKey);
    }
}
