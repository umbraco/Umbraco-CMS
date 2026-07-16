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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        var current = await ElementContainerService.GetAsync(root.Key);
        Assert.IsNull(current);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        child = await ElementContainerService.GetAsync(child.Key);
        Assert.IsNull(child);

        root = await ElementContainerService.GetAsync(root.Key);
        Assert.IsNotNull(root);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.NotEmpty, result.Status);
        });

        var current = await ElementContainerService.GetAsync(root.Key);
        Assert.IsNotNull(current);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await ElementContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.NotFound, result.Status);
        });
    }

    [Test]
    public async Task Container_Delete_Events_Are_Fired()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(container);

        try
        {
            EntityContainerNotificationHandler.DeletingContainer = notification =>
            {
                deletingWasCalled = true;
                Assert.AreEqual(containerKey, notification.DeletedEntities.Single().Key);
            };

            EntityContainerNotificationHandler.DeletedContainer = notification =>
            {
                deletedWasCalled = true;
                Assert.AreEqual(containerKey, notification.DeletedEntities.Single().Key);
            };

            var result = await ElementContainerService.DeleteAsync(containerKey, Constants.Security.SuperUserKey);

            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(deletingWasCalled);
            Assert.IsTrue(deletedWasCalled);
        }
        finally
        {
            EntityContainerNotificationHandler.DeletingContainer = null;
            EntityContainerNotificationHandler.DeletedContainer = null;
        }

        Assert.AreEqual(0, GetAtRoot().Length);
        Assert.IsNull(await ElementContainerService.GetAsync(containerKey));
    }

    [Test]
    public async Task Container_Delete_Event_Can_Be_Cancelled()
    {
        var deletingWasCalled = false;
        var deletedWasCalled = false;

        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "The Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.IsNotNull(container);

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

            Assert.AreEqual(EntityContainerOperationStatus.CancelledByNotification, result.Status);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(deletingWasCalled);
            Assert.IsFalse(deletedWasCalled);

            Assert.AreEqual(1, GetAtRoot().Length);
            Assert.IsNotNull(await ElementContainerService.GetAsync(containerKey));
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
        Assert.IsTrue(createResult.Success);
        EntityContainer container = createResult.Result!;
        IContentType elementType = await CreateElementType();
        IElement element = await CreateElement(elementType.Key, container.Key);

        // Trashing the element creates a "relate parent element container on element delete" relation whose parent is
        // the container node, so that the element can be restored to its original location later.
        var trashResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(trashResult.Success);
        Assert.IsNotEmpty(RelationService.GetByParentOrChildId(container.Id));

        // Deleting the now-empty container must clean up that relation, otherwise the FK on umbracoRelation is violated.
        var result = await ElementContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        Assert.IsNull(await ElementContainerService.GetAsync(container.Key));
        Assert.IsEmpty(RelationService.GetByParentOrChildId(container.Id));

        // The trashed element is untouched and, having lost its original-parent relation, is restorable to the root.
        IElement? trashedElement = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNotNull(trashedElement);
        Assert.IsTrue(trashedElement.Trashed);

        var originalParent = await ElementRecycleBinQueryService.GetOriginalParentAsync(element.Key);
        Assert.AreEqual(RecycleBinQueryResultType.NoParentRecycleRelation, originalParent.Status);
    }
}
