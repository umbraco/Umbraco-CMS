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
        Assert.That(GetFolderChildren(containerKey), Is.Empty);

        var element = await CreateInvariantElement();

        var moveResult = await ElementEditingService.MoveAsync(element.Key, containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.ParentId, Is.EqualTo(container.Id));
            Assert.That(element.Path, Is.EqualTo($"{container.Path},{element.Id}"));
        });

        var result = GetFolderChildren(containerKey);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result.First().Key, Is.EqualTo(element.Key));
    }

    [Test]
    public async Task Can_Move_Element_From_A_Folder_To_Root()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey);
        Assert.That(element.ParentId, Is.EqualTo(container.Id));
        Assert.That(GetFolderChildren(containerKey), Has.Length.EqualTo(1));

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.True);
            Assert.That(moveResult.Result, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(element.Path, Is.EqualTo($"{Constants.System.Root},{element.Id}"));
        });

        Assert.That(GetFolderChildren(containerKey), Is.Empty);
    }

    [Test]
    public async Task Can_Move_Element_Between_Folders()
    {
        var containerKey1 = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey);
        var containerKey2 = Guid.NewGuid();
        var container2 = (await ElementContainerService.CreateAsync(containerKey2, "Container #2", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey1);
        Assert.That(GetFolderChildren(containerKey1), Has.Length.EqualTo(1));

        await ElementEditingService.MoveAsync(element.Key, containerKey2, Constants.Security.SuperUserKey);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.ParentId, Is.EqualTo(container2.Id));
            Assert.That(element.Path, Is.EqualTo($"{container2.Path},{element.Id}"));
        });

        var result = GetFolderChildren(containerKey2);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result.First().Key, Is.EqualTo(element.Key));

        Assert.That(GetFolderChildren(containerKey1), Is.Empty);
    }

    [Test]
    public async Task Can_Move_Trashed_Element_To_Root()
    {
        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.That(moveResult.Success, Is.True);

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
    public async Task Can_Move_Trashed_Element_To_A_Folder()
    {
        var containerKey1 = Guid.NewGuid();
        var container1 = (await ElementContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement();

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, containerKey1, Constants.Security.SuperUserKey);
        Assert.That(moveResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.Trashed, Is.False);
            Assert.That(element.ParentId, Is.EqualTo(container1.Id));
            Assert.That(element.Path, Is.EqualTo($"{container1.Path},{element.Id}"));
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
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.Trashed, Is.True);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.That(moveResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.False);
        Assert.That(element.Trashed, Is.False);
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

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.PublishedCultures, Is.EquivalentTo(publishedCultures));
        Assert.That(element.Trashed, Is.True);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.That(moveResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.False);
        Assert.That(element.PublishedCultures, Is.Empty);
        Assert.That(element.Trashed, Is.False);
    }

    [Test]
    public async Task Can_Cancel_Unpublishing_When_Moving_Trashed_Published_Element()
    {
        var element = await CreateInvariantElement();

        var publishResult = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);

        var moveToRecycleBinResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Published, Is.True);
        Assert.That(element.Trashed, Is.True);

        try
        {
            ElementNotificationHandler.UnpublishingElement = (notification) => notification.Cancel = true;

            var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
            Assert.That(moveResult.Success, Is.True);

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
    public async Task Cannot_Move_Element_To_Container_In_Recycle_Bin()
    {
        var element = await CreateInvariantElement();

        var trashedContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(trashedContainerKey, "Trashed Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.MoveToRecycleBinAsync(trashedContainerKey, Constants.Security.SuperUserKey);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, trashedContainerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(ContentEditingOperationStatus.InTrash));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.False);

        Assert.That(GetFolderChildren(trashedContainerKey, true), Is.Empty);
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
            Assert.That(moveResult.Success, Is.False);
            Assert.That(moveResult.Result, Is.EqualTo(ContentEditingOperationStatus.InTrash));
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Trashed, Is.True);

        Assert.That(GetFolderChildren(trashedContainerKey, true), Is.Empty);
    }
}
