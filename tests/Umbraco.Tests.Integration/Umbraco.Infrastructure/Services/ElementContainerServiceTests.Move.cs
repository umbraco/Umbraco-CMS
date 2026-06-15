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
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var otherRootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(otherRootContainerKey, "Other Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", otherRootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);

        Assert.That(GetFolderChildren(childContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(otherRootContainerKey), Has.Length.EqualTo(1));

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, childContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.That(rootContainer, Is.Not.Null);
        var otherRootContainer = await ElementContainerService.GetAsync(otherRootContainerKey);
        Assert.That(otherRootContainer, Is.Not.Null);

        Assert.That(rootContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(rootContainer.Path, Is.EqualTo($"{Constants.System.Root},{otherRootContainer.Id},{childContainer.Id},{rootContainer.Id}"));
        Assert.That(rootContainer.Level, Is.EqualTo(childContainer.Level + 1));
    }

    [Test]
    public async Task Can_Move_Empty_Container_From_Another_Container_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(Constants.System.Root));
        Assert.That(childContainer.Path, Is.EqualTo($"{Constants.System.Root},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Another_Container_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(rootContainer, Is.Not.Null);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(4));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(Constants.System.Root));
        Assert.That(childContainer.Path, Is.EqualTo($"{Constants.System.Root},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(1));

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(2));

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(3));
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_Root_To_Another_Container()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(rootContainer, Is.Not.Null);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(Constants.System.Root));
        Assert.That(childContainer.Path, Is.EqualTo($"{Constants.System.Root},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(1));

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(2));

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(3));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(4));
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers_From_One_Container_To_Another_Container()
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(rootContainer, Is.Not.Null);

        var otherRootContainerKey = Guid.NewGuid();
        var otherRootContainer = (await ElementContainerService.CreateAsync(otherRootContainerKey, "Other Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(otherRootContainer, Is.Not.Null);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));

        var greatGrandchildContainerKey = Guid.NewGuid();
        var greatGrandchildContainer = (await ElementContainerService.CreateAsync(greatGrandchildContainerKey, "Great Grandchild Container", grandchildContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(4));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(otherRootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveAsync(childContainerKey, otherRootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);
        Assert.That(GetFolderChildren(otherRootContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(grandchildContainerKey), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(greatGrandchildContainerKey), Is.Empty);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(otherRootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{otherRootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        grandchildContainer = await ElementContainerService.GetAsync(grandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));

        greatGrandchildContainer = await ElementContainerService.GetAsync(greatGrandchildContainerKey);
        Assert.That(greatGrandchildContainer, Is.Not.Null);
        Assert.That(greatGrandchildContainer.ParentId, Is.EqualTo(grandchildContainer.Id));
        Assert.That(greatGrandchildContainer.Path, Is.EqualTo($"{grandchildContainer.Path},{greatGrandchildContainer.Id}"));
        Assert.That(greatGrandchildContainer.Level, Is.EqualTo(4));
    }

    [Test]
    [LongRunning]
    public async Task Can_Move_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Root_To_Another_Container()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(true);

        var moveResult = await ElementContainerService.MoveAsync(setup.ChildContainerKey, setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var rootContainer = await ElementContainerService.GetAsync(setup.RootContainerKey);
        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.That(rootContainer, Is.Not.Null);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(rootContainer.Id));
        Assert.That(childContainer.Path, Is.EqualTo($"{rootContainer.Path},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(2));

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(3));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(setup.RootContainerKey), Has.Length.EqualTo(1));

        var grandchildren = GetFolderChildren(setup.ChildContainerKey);
        Assert.That(grandchildren, Has.Length.EqualTo(setup.ChildContainerItems));

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey);
        Assert.That(greatGrandchildren, Has.Length.EqualTo(setup.GrandchildContainerItems));

        foreach (var element in grandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(childContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{childContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(3));
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(grandchildContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{grandchildContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(4));
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
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var childContainer = await ElementContainerService.GetAsync(setup.ChildContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.ParentId, Is.EqualTo(Constants.System.Root));
        Assert.That(childContainer.Path, Is.EqualTo($"{Constants.System.Root},{childContainer.Id}"));
        Assert.That(childContainer.Level, Is.EqualTo(1));

        var grandchildContainer = await ElementContainerService.GetAsync(setup.GrandchildContainerKey);
        Assert.That(grandchildContainer, Is.Not.Null);
        Assert.That(grandchildContainer.ParentId, Is.EqualTo(childContainer.Id));
        Assert.That(grandchildContainer.Path, Is.EqualTo($"{childContainer.Path},{grandchildContainer.Id}"));
        Assert.That(grandchildContainer.Level, Is.EqualTo(2));

        Assert.That(GetAtRoot(), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(setup.RootContainerKey), Is.Empty);

        var grandchildren = GetFolderChildren(setup.ChildContainerKey);
        Assert.That(grandchildren, Has.Length.EqualTo(setup.ChildContainerItems));

        var greatGrandchildren = GetFolderChildren(setup.GrandchildContainerKey);
        Assert.That(greatGrandchildren, Has.Length.EqualTo(setup.GrandchildContainerItems));

        foreach (var element in grandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(childContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{childContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(2));
        }

        foreach (var element in greatGrandchildren)
        {
            Assert.That(element.ParentId, Is.EqualTo(grandchildContainer.Id));
            Assert.That(element.Path, Is.EqualTo($"{grandchildContainer.Path},{element.Id}"));
            Assert.That(element.Level, Is.EqualTo(3));
        }
    }

    [Test]
    public async Task Can_Move_Container_With_Descendants_From_Recycle_Bin_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.That(childElement, Is.Not.Null);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.That(grandchildElement, Is.Not.Null);

        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);
        await AssertContainerIsInRecycleBin(rootContainerKey);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.Trashed, Is.True);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.That(childElement, Is.Not.Null);
        Assert.That(childElement.Trashed, Is.True);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.That(grandchildElement, Is.Not.Null);
        Assert.That(grandchildElement.Trashed, Is.True);

        Assert.That(GetFolderChildren(rootContainerKey, true), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(childContainerKey, true), Has.Length.EqualTo(1));

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var rootContainer = await ElementContainerService.GetAsync(rootContainerKey);
        Assert.That(rootContainer, Is.Not.Null);
        Assert.That(rootContainer.Trashed, Is.False);

        childContainer = await ElementContainerService.GetAsync(childContainerKey);
        Assert.That(childContainer, Is.Not.Null);
        Assert.That(childContainer.Trashed, Is.False);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.That(childElement, Is.Not.Null);
        Assert.That(childElement.Trashed, Is.False);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.That(grandchildElement, Is.Not.Null);
        Assert.That(grandchildElement.Trashed, Is.False);

        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));
    }

    [Test]
    public async Task Moving_Trashed_Container_Performs_Explicit_Unpublish_Of_All_Descendant_Elements()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.That(childElement, Is.Not.Null);

        var publishResult = await ElementPublishingService.PublishAsync(
            childElement.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.That(grandchildElement, Is.Not.Null);

        publishResult = await ElementPublishingService.PublishAsync(
            grandchildElement.Key,
            [new() { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);
        await AssertContainerIsInRecycleBin(rootContainerKey);

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.That(childElement, Is.Not.Null);
        Assert.That(childElement.Published, Is.True);
        Assert.That(childElement.Trashed, Is.True);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.That(grandchildElement, Is.Not.Null);
        Assert.That(grandchildElement.Published, Is.True);
        Assert.That(grandchildElement.Trashed, Is.True);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        childElement = await ElementEditingService.GetAsync(childElement.Key);
        Assert.That(childElement, Is.Not.Null);
        Assert.That(childElement.Published, Is.False);
        Assert.That(childElement.Trashed, Is.False);

        grandchildElement = await ElementEditingService.GetAsync(grandchildElement.Key);
        Assert.That(grandchildElement, Is.Not.Null);
        Assert.That(grandchildElement.Published, Is.False);
        Assert.That(grandchildElement.Trashed, Is.False);
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
                Assert.That(moveInfo.Entity.Key, Is.EqualTo(secondContainerKey));
                Assert.That(moveInfo.NewParentKey, Is.EqualTo(firstContainerKey));
            };

            EntityContainerNotificationHandler.MovedContainer = notification =>
            {
                movedWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.That(moveInfo.Entity.Key, Is.EqualTo(secondContainerKey));
                Assert.That(moveInfo.NewParentKey, Is.EqualTo(firstContainerKey));
            };

            var result = await ElementContainerService.MoveAsync(secondContainerKey, firstContainerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
            Assert.That(result.Success, Is.True);
            Assert.That(movingWasCalled, Is.True);
            Assert.That(movedWasCalled, Is.True);

            Assert.That(GetFolderChildren(firstContainerKey), Has.Length.EqualTo(1));
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
                Assert.That(moveInfo.Entity.Key, Is.EqualTo(secondContainerKey));
                Assert.That(moveInfo.NewParentKey, Is.EqualTo(firstContainerKey));

                notification.Cancel = true;
            };

            EntityContainerNotificationHandler.MovedContainer = _ =>
            {
                movedWasCalled = true;
            };

            var result = await ElementContainerService.MoveAsync(secondContainerKey, firstContainerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Result, Is.EqualTo(EntityContainerOperationStatus.CancelledByNotification));
            Assert.That(result.Success, Is.False);
            Assert.That(movingWasCalled, Is.True);
            Assert.That(movedWasCalled, Is.False);

            Assert.That(GetFolderChildren(firstContainerKey), Is.Empty);
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
        Assert.That(GetFolderChildren(rootContainerKey), Is.Empty);

        var moveResult = await ElementContainerService.MoveAsync(rootContainerKey, rootContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.InvalidParent));
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
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.InvalidParent));
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
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(EntityContainerOperationStatus.InvalidParent));
        });
    }
}
