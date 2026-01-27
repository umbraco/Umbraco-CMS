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
        Assert.IsTrue(moveToRecycleBinResult.Success);
        await AssertContainerIsInRecycleBin(containerKey);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(container.Trashed);
            Assert.AreEqual(Constants.System.Root, container.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{container.Id}", container.Path);
            Assert.AreEqual(1, container.Level);
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
        Assert.NotNull(targetContainer);

        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            containerKey,
            "Container",
            null,
            Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            targetContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(container.Trashed);
            Assert.AreEqual(targetContainer.Id, container.ParentId);
            Assert.AreEqual($"{targetContainer.Path},{container.Id}", container.Path);
            Assert.AreEqual(2, container.Level);
        });

        Assert.AreEqual(1, GetFolderChildren(targetContainerKey).Length);
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
        Assert.NotNull(childContainer);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.IsNotNull(childElement);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.IsNotNull(grandchildElement);

        Assert.AreEqual(2, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(1, GetFolderChildren(childContainerKey).Length);

        var moveToRecycleBinResult = await ElementContainerService.MoveToRecycleBinAsync(
            rootContainerKey,
            Constants.Security.SuperUserKey);
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

        var restoreResult = await ElementContainerService.RestoreAsync(
            rootContainerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, restoreResult.Status);
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
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.NotInTrash, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.IsFalse(container.Trashed);
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
        Assert.IsTrue(moveToRecycleBinResult.Success);

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
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.InTrash, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.IsTrue(container.Trashed);
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
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.NotFound, restoreResult.Status);
        });
    }

    [Test]
    public async Task Cannot_Restore_Container_To_NonExistent_Parent()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var nonExistentParentKey = Guid.NewGuid();

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            nonExistentParentKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.ParentNotFound, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.IsTrue(container.Trashed);
    }

    [Test]
    public async Task Cannot_Restore_Container_To_An_Element()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);

        var moveToRecycleBinResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var elementType = await CreateElementType();
        var targetElement = await CreateElement(elementType.Key);

        var restoreResult = await ElementContainerService.RestoreAsync(
            containerKey,
            targetElement.Key,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.ParentNotFound, restoreResult.Status);
        });

        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.IsTrue(container.Trashed);
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
        Assert.NotNull(childContainer);

        var elementType = await CreateElementType();

        var childElement = await CreateElement(elementType.Key, rootContainerKey);
        Assert.IsNotNull(childElement);

        var publishResult = await ElementPublishingService.PublishAsync(
            childElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var grandchildElement = await CreateElement(elementType.Key, childContainerKey);
        Assert.IsNotNull(grandchildElement);

        publishResult = await ElementPublishingService.PublishAsync(
            grandchildElement.Key,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementContainerService
            .MoveToRecycleBinAsync(rootContainerKey, Constants.Security.SuperUserKey);
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

        var restoreResult = await ElementContainerService.RestoreAsync(
            rootContainerKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, restoreResult.Status);
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
}
