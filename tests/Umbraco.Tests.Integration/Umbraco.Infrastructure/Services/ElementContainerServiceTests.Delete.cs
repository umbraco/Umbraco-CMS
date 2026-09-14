using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var current = await ElementContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Null);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        child = await ElementContainerService.GetAsync(child.Key);
        Assert.That(child, Is.Null);

        root = await ElementContainerService.GetAsync(root.Key);
        Assert.That(root, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));
        });

        var current = await ElementContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Element()
    {
        var createResult = await ElementContainerService.CreateAsync(null, "Container", null, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        EntityContainer container = createResult.Result!;
        IContentType elementType = await CreateElementType();
        await CreateElement(elementType.Key, container.Key);

        var result = await ElementContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));
        });

        Assert.That(await ElementContainerService.GetAsync(container.Key), Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await ElementContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotFound));
        });
    }

    [Test]
    public async Task Container_Delete_Events_Are_Fired()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                Assert.That(notification.DeletedEntities.Single().Key, Is.EqualTo(containerKey));
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedWasCalled = true;
                Assert.That(notification.DeletedEntities.Single().Key, Is.EqualTo(containerKey));
            };

            var result = await ElementContainerService.DeleteAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
            Assert.That(result.Success, Is.True);
            Assert.That(deletingWasCalled, Is.True);
            Assert.That(deletedWasCalled, Is.True);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        Assert.That(GetAtRoot(), Is.Empty);
        Assert.That(await ElementContainerService.GetAsync(containerKey), Is.Null);
    }

    [Test]
    public async Task Container_Delete_Event_Can_Be_Cancelled()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(container, Is.Not.Null);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                notification.Cancel = true;
            };

            EntityContainerNotificationHandler.DeletedContainer = _ =>
            {
                deletedWasCalled = true;
            };

            var result = await ElementContainerService.DeleteAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.CancelledByNotification));
            Assert.That(result.Success, Is.False);
            Assert.That(deletingWasCalled, Is.True);
            Assert.That(deletedWasCalled, Is.False);

            Assert.That(GetAtRoot(), Has.Length.EqualTo(1));
            Assert.That(await ElementContainerService.GetAsync(containerKey), Is.Not.Null);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }
    }

    [Test]
    public async Task Can_Delete_Container_After_Child_Element_Trashed()
    {
        var createResult = await ElementContainerService.CreateAsync(null, "Container", null, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        EntityContainer container = createResult.Result!;
        IContentType elementType = await CreateElementType();
        IElement element = await CreateElement(elementType.Key, container.Key);

        // Trashing the element creates a "relate parent element container on element delete" relation whose parent is
        // the container node, so that the element can be restored to its original location later.
        var trashResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);
        Assert.That(RelationService.GetByParentOrChildId(container.Id), Is.Not.Empty);

        // Deleting the now-empty container must clean up that relation, otherwise the FK on umbracoRelation is violated.
        var result = await ElementContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        Assert.That(await ElementContainerService.GetAsync(container.Key), Is.Null);
        Assert.That(RelationService.GetByParentOrChildId(container.Id), Is.Empty);

        // The trashed element is untouched and, having lost its original-parent relation, is restorable to the root.
        IElement? trashedElement = await ElementEditingService.GetAsync(element.Key);
        Assert.That(trashedElement, Is.Not.Null);
        Assert.That(trashedElement.Trashed, Is.True);

        var originalParent = await ElementRecycleBinQueryService.GetOriginalParentAsync(element.Key);
        Assert.That(originalParent.Status, Is.EqualTo(RecycleBinQueryResultType.NoParentRecycleRelation));
    }
}
