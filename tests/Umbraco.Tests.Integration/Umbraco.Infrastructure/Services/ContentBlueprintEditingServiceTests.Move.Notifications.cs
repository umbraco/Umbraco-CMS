using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentMovingBlueprintNotification, BlueprintMoveNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedBlueprintNotification, BlueprintMoveNotificationHandler>();
    }

    [SetUp]
    public void ResetBlueprintMoveNotificationHandler()
    {
        BlueprintMoveNotificationHandler.Moving = null;
        BlueprintMoveNotificationHandler.Moved = null;
    }

    [Test]
    public async Task Blueprint_Move_Events_Are_Fired()
    {
        var containerKey = Guid.NewGuid();
        EntityContainer container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result!;

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, null), Constants.Security.SuperUserKey);

        IContent? blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        var originalPath = blueprint.Path;

        MoveEventInfo<IContent>? movingInfo = null;
        MoveEventInfo<IContent>? movedInfo = null;
        BlueprintMoveNotificationHandler.Moving = notification => movingInfo = notification.MoveInfoCollection.Single();
        BlueprintMoveNotificationHandler.Moved = notification => movedInfo = notification.MoveInfoCollection.Single();

        Attempt<ContentEditingOperationStatus> result = await ContentBlueprintEditingService.MoveAsync(blueprintKey, containerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        Assert.Multiple(() =>
        {
            Assert.NotNull(movingInfo);
            Assert.NotNull(movedInfo);
            Assert.AreEqual(blueprintKey, movingInfo!.Entity.Key);
            Assert.AreEqual(originalPath, movingInfo.OriginalPath, "The moving notification should carry the path from before the move.");
            Assert.AreEqual(containerKey, movingInfo.NewParentKey);
            Assert.AreEqual(blueprintKey, movedInfo!.Entity.Key);
            Assert.AreEqual(originalPath, movedInfo.OriginalPath);
        });

        // the blueprint really did move
        IContent? moved = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(moved);
        Assert.AreEqual(container.Id, moved.ParentId);
    }

    [Test]
    public async Task Blueprint_Moving_Event_Can_Be_Cancelled()
    {
        var containerKey = Guid.NewGuid();
        await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, null), Constants.Security.SuperUserKey);

        IContent? blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        var originalParentId = blueprint.ParentId;
        var originalPath = blueprint.Path;

        var movedWasFired = false;
        BlueprintMoveNotificationHandler.Moving = notification => notification.Cancel = true;
        BlueprintMoveNotificationHandler.Moved = _ => movedWasFired = true;

        Attempt<ContentEditingOperationStatus> result = await ContentBlueprintEditingService.MoveAsync(blueprintKey, containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.CancelledByNotification, result.Result);
            Assert.IsFalse(movedWasFired, "The moved notification should not be published for a cancelled move.");
        });

        // the cancelled move must not have been persisted
        IContent? notMoved = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(notMoved);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(originalParentId, notMoved.ParentId);
            Assert.AreEqual(originalPath, notMoved.Path);
        });

        Assert.AreEqual(0, GetBlueprintChildren(containerKey).Length);
    }

    [Test]
    public async Task Moving_Blueprint_To_Its_Current_Parent_Does_Not_Fire_Events()
    {
        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, null), Constants.Security.SuperUserKey);

        var movingWasFired = false;
        var movedWasFired = false;
        BlueprintMoveNotificationHandler.Moving = _ => movingWasFired = true;
        BlueprintMoveNotificationHandler.Moved = _ => movedWasFired = true;

        // the blueprint is already at the root
        Attempt<ContentEditingOperationStatus> result = await ContentBlueprintEditingService.MoveAsync(blueprintKey, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsFalse(movingWasFired);
            Assert.IsFalse(movedWasFired);
        });
    }

    private sealed class BlueprintMoveNotificationHandler :
        INotificationHandler<ContentMovingBlueprintNotification>,
        INotificationHandler<ContentMovedBlueprintNotification>
    {
        public static Action<ContentMovingBlueprintNotification>? Moving { get; set; }

        public static Action<ContentMovedBlueprintNotification>? Moved { get; set; }

        public void Handle(ContentMovingBlueprintNotification notification) => Moving?.Invoke(notification);

        public void Handle(ContentMovedBlueprintNotification notification) => Moved?.Invoke(notification);
    }
}
