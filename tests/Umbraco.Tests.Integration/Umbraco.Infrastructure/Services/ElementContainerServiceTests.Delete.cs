using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

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
}
