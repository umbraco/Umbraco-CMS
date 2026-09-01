using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Restore_Container_To_Root()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            containerKey,
            "Container",
            null,
            Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);
        await AssertContainerIsInRecycleBin(containerKey);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(container.Trashed, Is.False);
            Assert.That(container.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(container.Path, Is.EqualTo($"{Constants.System.Root},{container.Id}"));
            Assert.That(container.Level, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Restore_Container_To_Another_Container()
    {
        var targetContainerKey = Guid.NewGuid();
        var targetContainer = (await ElementContainerService.CreateAsync(
            targetContainerKey,
            "Target Container",
            null,
            Constants.Security.SuperUserKey)).Result;
        Assert.That(targetContainer, Is.Not.Null);

        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            containerKey,
            "Container",
            null,
            Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            targetContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(container.Trashed, Is.False);
            Assert.That(container.ParentId, Is.EqualTo(targetContainer.Id));
            Assert.That(container.Path, Is.EqualTo($"{targetContainer.Path},{container.Id}"));
            Assert.That(container.Level, Is.EqualTo(2));
        });

        Assert.That(GetFolderChildren(targetContainerKey), Has.Length.EqualTo(1));
    }

    [Test]
    public async Task Can_Restore_Container_With_Descendants_To_Root()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(
            childContainerKey,
            "Child Container",
            rootContainerKey,
            Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.That(childElement, Is.Not.Null);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.That(grandchildElement, Is.Not.Null);

        Assert.That(GetFolderChildren(rootContainerKey), Has.Length.EqualTo(2));
        Assert.That(GetFolderChildren(childContainerKey), Has.Length.EqualTo(1));

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(
            rootContainerKey,
            Constants.Security.SuperUserKey);
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

        var restoreResult = await ElementContainerService.RestoreAsync(
            rootContainerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
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
    public async Task Cannot_Restore_Container_Not_In_Recycle_Bin()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            containerKey,
            "Container",
            null,
            Constants.Security.SuperUserKey);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.NotInTrash));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.That(container.Trashed, Is.False);
    }

    [Test]
    public async Task Cannot_Restore_Container_To_Trashed_Container()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            containerKey,
            "Container",
            null,
            Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var trashedTargetContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            trashedTargetContainerKey,
            "Trashed Target Container",
            null,
            Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedTargetContainerKey, Constants.Security.SuperUserKey);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            trashedTargetContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.InTrash));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.That(container.Trashed, Is.True);
    }

    [Test]
    public async Task Cannot_Restore_NonExistent_Container()
    {
        var nonExistentKey = Guid.NewGuid();

        var restoreResult = await ElementContainerService.RestoreAsync(
            nonExistentKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.NotFound));
        });
    }

    [Test]
    public async Task Cannot_Restore_Container_To_NonExistent_Parent()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var nonExistentParentKey = Guid.NewGuid();

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            nonExistentParentKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.That(container.Trashed, Is.True);
    }

    [Test]
    public async Task Cannot_Restore_Container_To_An_Element()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var elementType = await CreateElementType();
        var targetElement = await CreateElement(elementType.Key);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            targetElement.Key,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.That(container, Is.Not.Null);
        Assert.That(container.Trashed, Is.True);
    }

    [Test]
    public async Task Restoring_Container_Performs_Explicit_Unpublish_Of_All_Descendant_Elements()
    {
        var rootContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            rootContainerKey,
            "Root Container",
            null,
            Constants.Security.SuperUserKey);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(
            childContainerKey,
            "Child Container",
            rootContainerKey,
            Constants.Security.SuperUserKey)).Result;
        Assert.That(childContainer, Is.Not.Null);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.That(childElement, Is.Not.Null);

        var publishResult = await ElementPublishingService.PublishAsync(
            childElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.That(grandchildElement, Is.Not.Null);

        publishResult = await ElementPublishingService.PublishAsync(
            grandchildElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementContainerService
            .MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
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

        var restoreResult = await ElementContainerService.RestoreAsync(
            rootContainerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(EntityContainerOperationStatus.Success));
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
}
