using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ElementServiceNotificationWithCacheTests : UmbracoIntegrationTest
{
    private IContentType _contentType;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(Mock.Of<IRequestCache>()));

    [SetUp]
    public async Task SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;

        _contentType = ContentTypeBuilder.CreateBasicElementType();
        _contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ElementSavingNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementSavedNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementMovingNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementMovedNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementCopyingNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementCopiedNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementMovingToRecycleBinNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementMovedToRecycleBinNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementDeletingNotification, ElementNotificationHandler>()
        .AddNotificationHandler<ElementDeletedNotification, ElementNotificationHandler>();

    [Test]
    public async Task Saving_Saved_Get_Value()
    {
        var createAttempt = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Initial name" }
                ],
            },
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(createAttempt.Success);
            Assert.IsNotNull(createAttempt.Result.Content);
        });

        var savingWasCalled = false;
        var savedWasCalled = false;

        ElementNotificationHandler.SavingElement = notification =>
        {
            savingWasCalled = true;

            var saved = notification.SavedEntities.First();
            var element = ElementService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Initial name", element.Name);
            });
        };

        ElementNotificationHandler.SavedElement = notification =>
        {
            savedWasCalled = true;

            var saved = notification.SavedEntities.First();
            var element = ElementService.GetById(saved.Key)!;

            Assert.Multiple(() =>
            {
                Assert.AreEqual("Updated name", saved.Name);
                Assert.AreEqual("Updated name", element.Name);
            });
        };

        try
        {
            var updateAttempt = await ElementEditingService.UpdateAsync(
                createAttempt.Result.Content!.Key,
                new ElementUpdateModel
                {
                    Variants = [
                        new() { Name = "Updated name" }
                    ],
                },
                Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(updateAttempt.Success);
                Assert.IsNotNull(updateAttempt.Result.Content);
            });

            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.SavingElement = null;
            ElementNotificationHandler.SavedElement = null;
        }
    }

    [Test]
    public async Task Moving_Moved_Fires_Notifications()
    {
        var container = (await ElementContainerService.CreateAsync(null, "Target", null, Constants.Security.SuperUserKey)).Result;

        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var movingWasCalled = false;
        var movedWasCalled = false;

        ElementNotificationHandler.MovingElement = _ => movingWasCalled = true;
        ElementNotificationHandler.MovedElement = _ => movedWasCalled = true;

        try
        {
            var moveAttempt = await ElementEditingService.MoveAsync(element.Key, container.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(moveAttempt.Success);
                Assert.AreEqual(moveAttempt.Result, ContentEditingOperationStatus.Success);
            });

            Assert.IsTrue(movingWasCalled);
            Assert.IsTrue(movedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.MovingElement = null;
            ElementNotificationHandler.MovedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.AreEqual(container.Id, element.ParentId);
    }

    [Test]
    public async Task Moving_Can_Cancel_Move()
    {
        var container = (await ElementContainerService.CreateAsync(null, "Target", null, Constants.Security.SuperUserKey)).Result;

        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var movingWasCalled = false;
        var movedWasCalled = false;

        ElementNotificationHandler.MovingElement = notification =>
        {
            movingWasCalled = true;
            notification.Cancel = true;
        };
        ElementNotificationHandler.MovedElement = _ => movedWasCalled = true;

        try
        {
            var moveAttempt = await ElementEditingService.MoveAsync(element.Key, container.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(moveAttempt.Success);
                Assert.AreEqual(moveAttempt.Result, ContentEditingOperationStatus.CancelledByNotification);
            });

            Assert.IsTrue(movingWasCalled);
            Assert.IsFalse(movedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.MovingElement = null;
            ElementNotificationHandler.MovedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.AreEqual(Constants.System.Root, element.ParentId);
    }

    [Test]
    public async Task Delete_Fires_Notifications()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var deletingWasCalled = false;
        var deletedWasCalled = false;

        ElementNotificationHandler.DeletingElement = _ => deletingWasCalled = true;
        ElementNotificationHandler.DeletedElement = _ => deletedWasCalled = true;

        Assert.IsNotNull(EntityService.Get(element.Key, UmbracoObjectTypes.Element));

        try
        {
            var moveAttempt = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(moveAttempt.Success);
                Assert.AreEqual(ContentEditingOperationStatus.Success, moveAttempt.Status);
            });

            Assert.IsTrue(deletingWasCalled);
            Assert.IsTrue(deletedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.DeletingElement = null;
            ElementNotificationHandler.DeletedElement = null;
        }

        Assert.IsNull(EntityService.Get(element.Key, UmbracoObjectTypes.Element));
   }

    [Test]
    public async Task Delete_Can_Cancel_Deletion()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var deletingWasCalled = false;
        var deletedWasCalled = false;

        ElementNotificationHandler.DeletingElement = notification =>
        {
            notification.Cancel = true;
            deletingWasCalled = true;
        };
        ElementNotificationHandler.DeletedElement = _ => deletedWasCalled = true;

        try
        {
            var deleteAttempt = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(deleteAttempt.Success);
                Assert.AreEqual(ContentEditingOperationStatus.CancelledByNotification, deleteAttempt.Status);
            });

            Assert.IsTrue(deletingWasCalled);
            Assert.IsFalse(deletedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.DeletingElement = null;
            ElementNotificationHandler.DeletedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.NotNull(element);
    }

    [Test]
    public async Task Moving_To_Recycle_Bin_Moved_Fires_Notifications()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        Assert.IsNotNull(element);
        var elementKey = element.Key;
        var elementPath = element.Path;

        var movingWasCalled = false;
        var movedWasCalled = false;

        try
        {
            ElementNotificationHandler.MovingElementToRecycleBin = notification =>
            {
                movingWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(elementKey, moveInfo.Entity.Key);
                Assert.AreEqual(elementPath, moveInfo.OriginalPath);
            };
            ElementNotificationHandler.MovedElementToRecycleBin = notification =>
            {
                movedWasCalled = true;
                var moveInfo = notification.MoveInfoCollection.Single();
                Assert.AreEqual(elementKey, moveInfo.Entity.Key);
                Assert.AreEqual(elementPath, moveInfo.OriginalPath);
            };

            var moveAttempt = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(moveAttempt.Success);
                Assert.AreEqual(moveAttempt.Result, ContentEditingOperationStatus.Success);
            });

            Assert.IsTrue(movingWasCalled);
            Assert.IsTrue(movedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.MovingElement = null;
            ElementNotificationHandler.MovedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.AreEqual(Constants.System.RecycleBinElement, element.ParentId);
    }

    [Test]
    public async Task Moving_To_Recycle_Bin_Can_Cancel_Move()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var movingWasCalled = false;
        var movedWasCalled = false;

        ElementNotificationHandler.MovingElementToRecycleBin = notification =>
        {
            movingWasCalled = true;
            notification.Cancel = true;
        };
        ElementNotificationHandler.MovedElementToRecycleBin = _ => movedWasCalled = true;

        try
        {
            var moveAttempt = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(moveAttempt.Success);
                Assert.AreEqual(moveAttempt.Result, ContentEditingOperationStatus.CancelledByNotification);
            });

            Assert.IsTrue(movingWasCalled);
            Assert.IsFalse(movedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.MovingElement = null;
            ElementNotificationHandler.MovedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.AreEqual(Constants.System.Root, element.ParentId);
    }

    [Test]
    public async Task Delete_From_Recycle_Bin_Fires_Notifications()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

        var deletingWasCalled = false;
        var deletedWasCalled = false;

        ElementNotificationHandler.DeletingElement = _ => deletingWasCalled = true;
        ElementNotificationHandler.DeletedElement = _ => deletedWasCalled = true;

        try
        {
            var moveAttempt = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(moveAttempt.Success);
                Assert.AreEqual(ContentEditingOperationStatus.Success, moveAttempt.Status);
            });

            Assert.IsTrue(deletingWasCalled);
            Assert.IsTrue(deletedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.DeletingElement = null;
            ElementNotificationHandler.DeletedElement = null;
        }
    }

    [Test]
    public async Task Delete_From_Recycle_Bin_Can_Cancel_Deletion()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

        var deletingWasCalled = false;
        var deletedWasCalled = false;

        ElementNotificationHandler.DeletingElement = notification =>
        {
            notification.Cancel = true;
            deletingWasCalled = true;
        };
        ElementNotificationHandler.DeletedElement = _ => deletedWasCalled = true;

        try
        {
            var deleteAttempt = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(deleteAttempt.Success);
                Assert.AreEqual(ContentEditingOperationStatus.CancelledByNotification, deleteAttempt.Status);
            });

            Assert.IsTrue(deletingWasCalled);
            Assert.IsFalse(deletedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.DeletingElement = null;
            ElementNotificationHandler.DeletedElement = null;
        }

        element = (await ElementEditingService.GetAsync(element.Key))!;
        Assert.NotNull(element);
    }

    [Test]
    public async Task Copying_Copied_Fires_Notifications()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var copyingWasCalled = false;
        var copiedWasCalled = false;

        ElementNotificationHandler.CopyingElement = _ => copyingWasCalled = true;
        ElementNotificationHandler.CopiedElement = _ => copiedWasCalled = true;

        try
        {
            var copyAttempt = await ElementEditingService.CopyAsync(element.Key, null, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(copyAttempt.Success);
                Assert.AreEqual(copyAttempt.Status, ContentEditingOperationStatus.Success);
            });

            Assert.IsTrue(copyingWasCalled);
            Assert.IsTrue(copiedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.CopyingElement = null;
            ElementNotificationHandler.CopiedElement = null;
        }
    }

    [Test]
    public async Task Copying_Can_Cancel_Copy()
    {
        var element = (await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _contentType.Key,
                Variants = [
                    new() { Name = "Name" }
                ],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var copyingWasCalled = false;
        var copiedWasCalled = false;

        ElementNotificationHandler.CopyingElement = notification =>
        {
            notification.Cancel = true;
            copyingWasCalled = true;
        };
        ElementNotificationHandler.CopiedElement = _ => copiedWasCalled = true;

        try
        {
            var copyAttempt = await ElementEditingService.CopyAsync(element.Key, null, Constants.Security.SuperUserKey);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(copyAttempt.Success);
                Assert.AreEqual(copyAttempt.Status, ContentEditingOperationStatus.CancelledByNotification);
            });

            Assert.IsTrue(copyingWasCalled);
            Assert.IsFalse(copiedWasCalled);
        }
        finally
        {
            ElementNotificationHandler.CopyingElement = null;
            ElementNotificationHandler.CopiedElement = null;
        }

        Assert.AreEqual(1, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());
    }

    internal sealed class ElementNotificationHandler :
        INotificationHandler<ElementSavingNotification>,
        INotificationHandler<ElementSavedNotification>,
        INotificationHandler<ElementMovingNotification>,
        INotificationHandler<ElementMovedNotification>,
        INotificationHandler<ElementCopyingNotification>,
        INotificationHandler<ElementCopiedNotification>,
        INotificationHandler<ElementMovingToRecycleBinNotification>,
        INotificationHandler<ElementMovedToRecycleBinNotification>,
        INotificationHandler<ElementDeletingNotification>,
        INotificationHandler<ElementDeletedNotification>
    {
        public static Action<ElementSavingNotification>? SavingElement { get; set; }

        public static Action<ElementSavedNotification>? SavedElement { get; set; }

        public static Action<ElementMovingNotification>? MovingElement { get; set; }

        public static Action<ElementMovedNotification>? MovedElement { get; set; }

        public static Action<ElementCopyingNotification>? CopyingElement { get; set; }

        public static Action<ElementCopiedNotification>? CopiedElement { get; set; }

        public static Action<ElementMovingToRecycleBinNotification>? MovingElementToRecycleBin { get; set; }

        public static Action<ElementMovedToRecycleBinNotification>? MovedElementToRecycleBin { get; set; }

        public static Action<ElementDeletingNotification>? DeletingElement { get; set; }

        public static Action<ElementDeletedNotification>? DeletedElement { get; set; }

        public void Handle(ElementSavedNotification notification) => SavedElement?.Invoke(notification);

        public void Handle(ElementSavingNotification notification) => SavingElement?.Invoke(notification);

        public void Handle(ElementMovingNotification notification) => MovingElement?.Invoke(notification);

        public void Handle(ElementMovedNotification notification) => MovedElement?.Invoke(notification);

        public void Handle(ElementCopyingNotification notification) => CopyingElement?.Invoke(notification);

        public void Handle(ElementCopiedNotification notification) => CopiedElement?.Invoke(notification);

        public void Handle(ElementMovingToRecycleBinNotification notification) => MovingElementToRecycleBin?.Invoke(notification);

        public void Handle(ElementMovedToRecycleBinNotification notification) => MovedElementToRecycleBin?.Invoke(notification);

        public void Handle(ElementDeletingNotification notification) => DeletingElement?.Invoke(notification);

        public void Handle(ElementDeletedNotification notification) => DeletedElement?.Invoke(notification);
    }
}
