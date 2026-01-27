using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Restore_Element_To_Root()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Trashed);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(element.Trashed);
            Assert.AreEqual(Constants.System.Root, element.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{element.Id}", element.Path);
        });
    }

    [Test]
    public async Task Can_Restore_Element_To_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(
            containerKey,
            "Target Container",
            null,
            Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Trashed);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            containerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(restoreResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(element.Trashed);
            Assert.AreEqual(container.Id, element.ParentId);
            Assert.AreEqual($"{container.Path},{element.Id}", element.Path);
        });

        var folderChildren = GetFolderChildren(containerKey);
        Assert.AreEqual(1, folderChildren.Length);
        Assert.AreEqual(element.Key, folderChildren.First().Key);
    }

    [Test]
    public async Task Cannot_Restore_Element_Not_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotInTrash, restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsFalse(element.Trashed);
    }

    [Test]
    public async Task Cannot_Restore_Element_To_Container_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var trashedContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(
            trashedContainerKey,
            "Trashed Container",
            null,
            Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedContainerKey, Constants.Security.SuperUserKey);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            trashedContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.InTrash, restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(element);
        Assert.IsTrue(element.Trashed);

        Assert.AreEqual(0, GetFolderChildren(trashedContainerKey, true).Length);
    }

    [Test]
    public async Task Restoring_Published_Invariant_Element_Performs_Explicit_Unpublish()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        Assert.IsTrue(element.Trashed);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(restoreResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsFalse(element.Published);
        Assert.IsFalse(element.Trashed);
    }

    [TestCase("en-US", "da-DK")]
    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Restoring_Published_Variant_Element_Performs_Explicit_Unpublish(params string[] publishedCultures)
    {
        var elementType = await CreateVariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The Invariant Title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish Title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "The English Name" },
                new VariantModel { Culture = "da-DK", Name = "The Danish Name" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(
            createModel,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var element = result.Result.Content!;

        var culturePublishScheduleModels = publishedCultures
            .Select(culture => new CulturePublishScheduleModel { Culture = culture })
            .ToArray();
        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            culturePublishScheduleModels,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        CollectionAssert.AreEquivalent(publishedCultures, element.PublishedCultures);
        Assert.IsTrue(element.Trashed);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(restoreResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsFalse(element.Published);
        Assert.IsEmpty(element.PublishedCultures);
        Assert.IsFalse(element.Trashed);
    }

    [Test]
    public async Task Can_Cancel_Unpublishing_When_Restoring_Published_Element()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        Assert.IsTrue(element.Trashed);

        try
        {
            ElementNotificationHandler.UnpublishingElement = (notification) => notification.Cancel = true;

            var restoreResult = await ElementEditingService.RestoreAsync(
                element.Key,
                null,
                Constants.Security.SuperUserKey);
            Assert.IsTrue(restoreResult.Success);

            element = await ElementEditingService.GetAsync(element.Key);
            Assert.NotNull(element);
            Assert.IsTrue(element.Published);
            Assert.IsFalse(element.Trashed);
        }
        finally
        {
            ElementNotificationHandler.UnpublishingElement = null;
        }
    }

    [Test]
    public async Task Cannot_Restore_NonExistent_Element()
    {
        var nonExistentKey = Guid.NewGuid();

        var restoreResult = await ElementEditingService.RestoreAsync(
            nonExistentKey,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(
                ContentEditingOperationStatus.NotFound,
                restoreResult.Status);
        });
    }

    [Test]
    public async Task Cannot_Restore_Element_To_NonExistent_Container()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var nonExistentContainerKey = Guid.NewGuid();

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            nonExistentContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(
                ContentEditingOperationStatus.ParentNotFound,
                restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(element);
        Assert.IsTrue(element.Trashed);
    }

    [Test]
    public async Task Cannot_Restore_Element_To_An_Element()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var targetElement = await CreateInvariantElement();

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            targetElement.Key,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoreResult.Success);
            Assert.AreEqual(
                ContentEditingOperationStatus.ParentNotFound,
                restoreResult.Status);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(element);
        Assert.IsTrue(element.Trashed);
    }
}
