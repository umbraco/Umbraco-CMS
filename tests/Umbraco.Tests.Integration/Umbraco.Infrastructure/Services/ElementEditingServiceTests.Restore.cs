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
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.Trashed, Is.False);
            Assert.That(element.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(element.Path, Is.EqualTo($"{Constants.System.Root},{element.Id}"));
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
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            containerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.True);
            Assert.That(restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.Trashed, Is.False);
            Assert.That(element.ParentId, Is.EqualTo(container.Id));
            Assert.That(element.Path, Is.EqualTo($"{container.Path},{element.Id}"));
        });

        var folderChildren = GetFolderChildren(containerKey);
        Assert.That(folderChildren, Has.Length.EqualTo(1));
        Assert.That(folderChildren.First().Key, Is.EqualTo(element.Key));
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
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.NotInTrash));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.False);
    }

    [Test]
    public async Task Cannot_Restore_Element_To_Container_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

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
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.InTrash));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);

        Assert.That(GetFolderChildren(trashedContainerKey, true), Is.Empty);
    }

    [Test]
    public async Task Restoring_Published_Invariant_Element_Performs_Explicit_Unpublish()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.Trashed, Is.True);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.That(restoreResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.False);
        Assert.That(element.Trashed, Is.False);
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
        Assert.That(result.Success, Is.True);
        var element = result.Result.Content!;

        var culturePublishScheduleModels = publishedCultures
            .Select(culture => new CulturePublishScheduleModel { Culture = culture })
            .ToArray();
        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            culturePublishScheduleModels,
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.PublishedCultures, Is.EquivalentTo(publishedCultures));
        Assert.That(element.Trashed, Is.True);

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            null,
            Constants.Security.SuperUserKey);
        Assert.That(restoreResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.False);
        Assert.That(element.PublishedCultures, Is.Empty);
        Assert.That(element.Trashed, Is.False);
    }

    [Test]
    public async Task Can_Cancel_Unpublishing_When_Restoring_Published_Element()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.Trashed, Is.True);

        try
        {
            ElementNotificationHandler.UnpublishingElement = (notification) => notification.Cancel = true;

            var restoreResult = await ElementEditingService.RestoreAsync(
                element.Key,
                null,
                Constants.Security.SuperUserKey);
            Assert.That(restoreResult.Success, Is.True);

            element = await ElementEditingService.GetAsync(element.Key);
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Published, Is.True);
            Assert.That(element.Trashed, Is.False);
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
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(
                restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.NotFound));
        });
    }

    [Test]
    public async Task Cannot_Restore_Element_To_NonExistent_Container()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var nonExistentContainerKey = Guid.NewGuid();

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            nonExistentContainerKey,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(
                restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);
    }

    [Test]
    public async Task Cannot_Restore_Element_To_An_Element()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(
            element.Key,
            Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var targetElement = await CreateInvariantElement();

        var restoreResult = await ElementEditingService.RestoreAsync(
            element.Key,
            targetElement.Key,
            Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(restoreResult.Success, Is.False);
            Assert.That(
                restoreResult.Result, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);
    }
}
