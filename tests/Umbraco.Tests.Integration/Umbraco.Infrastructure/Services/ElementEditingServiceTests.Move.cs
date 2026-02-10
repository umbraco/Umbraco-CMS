using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Move_Element_From_Root_To_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.AreEqual(0, GetFolderChildren(containerKey).Length);

        var element = await CreateInvariantElement();

        var moveResult = await ElementEditingService.MoveAsync(element.Key, containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container.Id, element.ParentId);
            Assert.AreEqual($"{container.Path},{element.Id}", element.Path);
        });

        var result = GetFolderChildren(containerKey);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(element.Key, result.First().Key);
    }

    [Test]
    public async Task Can_Move_Element_From_A_Folder_To_Root()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey);
        Assert.AreEqual(container.Id, element.ParentId);
        Assert.AreEqual(1, GetFolderChildren(containerKey).Length);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.Root, element.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{element.Id}", element.Path);
        });

        Assert.AreEqual(0, GetFolderChildren(containerKey).Length);
    }

    [Test]
    public async Task Can_Move_Element_Between_Folders()
    {
        var containerKey1 = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey);
        var containerKey2 = Guid.NewGuid();
        var container2 = (await ElementContainerService.CreateAsync(containerKey2, "Container #2", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey1);
        Assert.AreEqual(1, GetFolderChildren(containerKey1).Length);

        await ElementEditingService.MoveAsync(element.Key, containerKey2, Constants.Security.SuperUserKey);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container2.Id, element.ParentId);
            Assert.AreEqual($"{container2.Path},{element.Id}", element.Path);
        });

        var result = GetFolderChildren(containerKey2);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(element.Key, result.First().Key);

        Assert.AreEqual(0, GetFolderChildren(containerKey1).Length);
    }

    [Test]
    public async Task Can_Move_Trashed_Element_To_Root()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success);

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
    public async Task Can_Move_Trashed_Element_To_A_Folder()
    {
        var containerKey1 = Guid.NewGuid();
        var container1 = (await ElementContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, containerKey1, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(element.Trashed);
            Assert.AreEqual(container1.Id, element.ParentId);
            Assert.AreEqual($"{container1.Path},{element.Id}", element.Path);
        });
    }

    [Test]
    public async Task Moving_Trashed_Published_Invariant_Element_Performs_Explicit_Unpublish()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        Assert.IsTrue(element.Trashed);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsFalse(element.Published);
        Assert.IsFalse(element.Trashed);
    }

    [TestCase("en-US", "da-DK")]
    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Moving_Trashed_Published_Variant_Element_Performs_Explicit_Unpublish(params string[] publishedCultures)
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

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
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

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        CollectionAssert.AreEquivalent(publishedCultures, element.PublishedCultures);
        Assert.IsTrue(element.Trashed);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsFalse(element.Published);
        Assert.IsEmpty(element.PublishedCultures);
        Assert.IsFalse(element.Trashed);
    }

    [Test]
    public async Task Can_Cancel_Unpublishing_When_Moving_Trashed_Published_Element()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveToRecycleBinResult.Success);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.IsTrue(element.Published);
        Assert.IsTrue(element.Trashed);

        try
        {
            ElementNotificationHandler.UnpublishingElement = (notification) => notification.Cancel = true;

            var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
            Assert.IsTrue(moveResult.Success);

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
    public async Task Cannot_Move_Element_To_Container_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();

        var trashedContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(trashedContainerKey, "Trashed Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, trashedContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.InTrash, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(element);
        Assert.IsFalse(element.Trashed);

        Assert.AreEqual(0, GetFolderChildren(trashedContainerKey, true).Length);
    }

    [Test]
    public async Task Cannot_Move_Trashed_Element_To_Container_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();
        await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

        var trashedContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(trashedContainerKey, "Trashed Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, trashedContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.InTrash, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(element);
        Assert.IsTrue(element.Trashed);

        Assert.AreEqual(0, GetFolderChildren(trashedContainerKey, true).Length);
    }
}
