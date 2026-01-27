using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public partial class ElementContainerServiceTests : UmbracoIntegrationTest
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<EntityContainerMovingNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<EntityContainerMovedNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<EntityContainerMovingToRecycleBinNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<EntityContainerMovedToRecycleBinNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<EntityContainerDeletingNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<EntityContainerDeletedNotification, EntityContainerNotificationHandler>()
        .AddNotificationHandler<ElementDeletedNotification, ElementNotificationHandler>();

    private IEntitySlim[] GetAtRoot()
        => EntityService.GetRootEntities(UmbracoObjectTypes.ElementContainer).Union(EntityService.GetRootEntities(UmbracoObjectTypes.Element)).ToArray();

    private IEntitySlim[] GetFolderChildren(Guid containerKey, bool trashed = false)
        => EntityService.GetPagedChildren(containerKey, [UmbracoObjectTypes.ElementContainer], [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element], 0, 999, trashed, out _).ToArray();

    private async Task<IContentType> CreateElementType()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("test")
            .WithName("Test")
            .WithAllowAsRoot(true)
            .WithIsElement(true)
            .Build();

        var result = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.AreEqual(true, result.Success);
        return elementType;
    }

    private async Task<IElement> CreateElement(Guid contentTypeKey, Guid? parentKey = null)
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = contentTypeKey,
            ParentKey = parentKey,
            Variants =
            [
                new VariantModel { Name = Guid.NewGuid().ToString("N") }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<FolderWithElementsStructureInfo> CreateContainerWithDescendantContainersAndLotsOfElements(bool createChildContainerAtRoot)
    {
        var rootContainerKey = Guid.NewGuid();
        var rootContainer = (await ElementContainerService.CreateAsync(rootContainerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(rootContainer);

        var childContainerKey = Guid.NewGuid();
        var childContainer = (await ElementContainerService.CreateAsync(childContainerKey, "Child Container", createChildContainerAtRoot ? null : rootContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(childContainer);
        Assert.AreEqual(createChildContainerAtRoot ? Constants.System.Root : rootContainer.Id, childContainer.ParentId);
        Assert.AreEqual($"{(createChildContainerAtRoot ? Constants.System.Root : rootContainer.Path)},{childContainer.Id}", childContainer.Path);
        Assert.AreEqual(createChildContainerAtRoot ? 1 : 2, childContainer.Level);

        var grandchildContainerKey = Guid.NewGuid();
        var grandchildContainer = (await ElementContainerService.CreateAsync(grandchildContainerKey, "Grandchild Container", childContainerKey, Constants.Security.SuperUserKey)).Result;
        Assert.NotNull(grandchildContainer);

        var elementType = await CreateElementType();

        // ensure that we have at least three pages of descendants to iterate across
        var iterations = Cms.Core.Services.ElementContainerService.DescendantsIteratorPageSize + 5;
        for (var i = 0; i < iterations; i++)
        {
            var element = await CreateElement(elementType.Key, childContainerKey);
            Assert.AreEqual(childContainer.Id, element.ParentId);
            Assert.AreEqual($"{childContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(childContainer.Level + 1, element.Level);

            element = await CreateElement(elementType.Key, grandchildContainerKey);
            Assert.AreEqual(grandchildContainer.Id, element.ParentId);
            Assert.AreEqual($"{grandchildContainer.Path},{element.Id}", element.Path);
            Assert.AreEqual(grandchildContainer.Level + 1, element.Level);
        }

        Assert.AreEqual(createChildContainerAtRoot ? 2 : 1, GetAtRoot().Length);
        Assert.AreEqual(createChildContainerAtRoot ? 0 : 1, GetFolderChildren(rootContainerKey).Length);
        Assert.AreEqual(506, GetFolderChildren(childContainerKey).Length);
        Assert.AreEqual(505, GetFolderChildren(grandchildContainerKey).Length);

        return new()
        {
            RootContainerKey = rootContainerKey,
            ChildContainerKey = childContainerKey,
            GrandchildContainerKey = grandchildContainerKey,
            RootItems = createChildContainerAtRoot ? 2 : 1,
            RootContainerItems = createChildContainerAtRoot ? 0 : 1,
            ChildContainerItems = 506,
            GrandchildContainerItems = 505,
        };
    }

    private async Task AssertContainerIsInRecycleBin(Guid containerKey)
    {
        var container = await ElementContainerService.GetAsync(containerKey);
        Assert.NotNull(container);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.RecycleBinElement, container.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinElementPathPrefix}{container.Id}", container.Path);
            Assert.IsTrue(container.Trashed);
        });

        var recycleBinItems = EntityService
            .GetPagedChildren(Constants.System.RecycleBinElementKey, [UmbracoObjectTypes.ElementContainer], [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element], 0, 999, true, out var total)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, total);
            Assert.AreEqual(1, recycleBinItems.Length);
        });

        Assert.AreEqual(container.Key, recycleBinItems[0].Key);
    }

    private struct FolderWithElementsStructureInfo
    {
        public Guid RootContainerKey { get; init; }

        public Guid ChildContainerKey { get; init; }

        public Guid GrandchildContainerKey { get; init; }

        public int RootItems { get; init; }

        public int RootContainerItems { get; init; }

        public int ChildContainerItems { get; init; }

        public int GrandchildContainerItems { get; init; }
    }

    private sealed class EntityContainerNotificationHandler :
        INotificationHandler<EntityContainerMovingNotification>,
        INotificationHandler<EntityContainerMovedNotification>,
        INotificationHandler<EntityContainerMovingToRecycleBinNotification>,
        INotificationHandler<EntityContainerMovedToRecycleBinNotification>,
        INotificationHandler<EntityContainerDeletingNotification>,
        INotificationHandler<EntityContainerDeletedNotification>
    {
        public static Action<EntityContainerMovingNotification>? MovingContainer { get; set; }

        public static Action<EntityContainerMovedNotification>? MovedContainer { get; set; }

        public static Action<EntityContainerMovingToRecycleBinNotification>? MovingContainerToRecycleBin { get; set; }

        public static Action<EntityContainerMovedToRecycleBinNotification>? MovedContainerToRecycleBin { get; set; }

        public static Action<EntityContainerDeletingNotification>? DeletingContainer { get; set; }

        public static Action<EntityContainerDeletedNotification>? DeletedContainer { get; set; }

        public void Handle(EntityContainerMovingNotification notification) => MovingContainer?.Invoke(notification);

        public void Handle(EntityContainerMovedNotification notification) => MovedContainer?.Invoke(notification);

        public void Handle(EntityContainerMovingToRecycleBinNotification notification) => MovingContainerToRecycleBin?.Invoke(notification);

        public void Handle(EntityContainerMovedToRecycleBinNotification notification) => MovedContainerToRecycleBin?.Invoke(notification);

        public void Handle(EntityContainerDeletingNotification notification) => DeletingContainer?.Invoke(notification);

        public void Handle(EntityContainerDeletedNotification notification) => DeletedContainer?.Invoke(notification);
    }

    private sealed class ElementNotificationHandler : INotificationHandler<ElementDeletedNotification>
    {
        public static Action<ElementDeletedNotification>? DeletedElement { get; set; }

        public void Handle(ElementDeletedNotification notification) => DeletedElement?.Invoke(notification);
    }
}
