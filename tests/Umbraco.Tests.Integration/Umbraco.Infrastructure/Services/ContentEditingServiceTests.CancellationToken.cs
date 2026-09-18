using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    /// <summary>
    /// Shared, per-test-scope state that lets a test cancel a <see cref="System.Threading.CancellationTokenSource"/>
    /// deterministically from inside a <see cref="ContentCopyingNotification"/> handler, at a known point in the
    /// recursive copy, rather than racing a timer.
    /// </summary>
    public class CopyCancellationCoordinator
    {
        public System.Threading.CancellationTokenSource? CancellationTokenSource { get; set; }

        public int CancelAfterNotificationCount { get; set; } = int.MaxValue;

        public int NotificationCount { get; private set; }

        public void OnContentCopyingNotification()
        {
            NotificationCount++;
            if (NotificationCount == CancelAfterNotificationCount)
            {
                CancellationTokenSource?.Cancel();
            }
        }
    }

    public class CopyCancellationNotificationHandler : INotificationHandler<ContentCopyingNotification>
    {
        private readonly CopyCancellationCoordinator _coordinator;

        public CopyCancellationNotificationHandler(CopyCancellationCoordinator coordinator) => _coordinator = coordinator;

        public void Handle(ContentCopyingNotification notification) => _coordinator.OnContentCopyingNotification();
    }

    [Test]
    public async Task Cancelling_During_Copy_With_Descendants_Leaves_No_Partial_State()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent source, IContent child1) = await CreateRootAndChildAsync(contentType, "Source", "Child 1");

        // Add two more children so the recursive copy has several descendants to get through before cancellation.
        var child2Result = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = source.Key,
                Variants = [new() { Name = "Child 2" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(child2Result.Success);

        var child3Result = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = source.Key,
                Variants = [new() { Name = "Child 3" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(child3Result.Success);

        var destinationResult = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new() { Name = "Destination" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(destinationResult.Success);
        IContent destination = destinationResult.Result.Content!;

        var coordinator = GetRequiredService<CopyCancellationCoordinator>();
        using var cts = new System.Threading.CancellationTokenSource();
        coordinator.CancellationTokenSource = cts;

        // Notification #1 is for the root copy itself (fired before the descendants loop even starts); notification
        // #2 is the first descendant. Cancelling on #2 guarantees the root and at least one descendant are written
        // - inside the still-open scope - before the *next* loop iteration's ThrowIfCancellationRequested() throws.
        // A test that cancelled on #1 would only prove the boundary check, not that mid-loop writes get rolled back.
        coordinator.CancelAfterNotificationCount = 2;

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await ContentEditingService.CopyAsync(source.Key, destination.Key, false, true, Constants.Security.SuperUserKey, cts.Token));

        Assert.GreaterOrEqual(
            coordinator.NotificationCount,
            2,
            "Expected at least one descendant to start copying before cancellation - otherwise this test only proves the boundary check, not mid-operation rollback.");

        var destinationChildren = ContentService.GetPagedChildren(destination.Id, 0, 100, out var total, propertyAliases: null, filter: null, ordering: null).ToArray();
        Assert.AreEqual(0, destinationChildren.Length, "The scope must roll back everything written before the cancellation, including the root copy and any descendants already saved.");
        Assert.AreEqual(0, total);
    }

    [Test]
    public async Task Copy_With_Already_Cancelled_Token_Writes_Nothing()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var rootLevelChildrenBefore = ContentService.GetPagedChildren(Constants.System.Root, 0, 100, out var totalBefore, propertyAliases: null, filter: null, ordering: null).ToArray();

        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        // includeDescendants: false is deliberate - it keeps the descendants loop from ever running, so this can
        // only pass because of the boundary check at the top of Copy. With includeDescendants: true the loop's own
        // per-descendant check would also catch a pre-cancelled token, masking a missing boundary check entirely.
        Assert.CatchAsync<OperationCanceledException>(async () =>
            await ContentEditingService.CopyAsync(root.Key, Constants.System.RootKey, false, false, Constants.Security.SuperUserKey, cts.Token));

        var rootLevelChildrenAfter = ContentService.GetPagedChildren(Constants.System.Root, 0, 100, out var totalAfter, propertyAliases: null, filter: null, ordering: null).ToArray();
        Assert.AreEqual(totalBefore, totalAfter, "An already-cancelled token must fail before any write, so the root-level child count must be unchanged.");
        Assert.AreEqual(rootLevelChildrenBefore.Select(c => c.Key), rootLevelChildrenAfter.Select(c => c.Key));
    }

    [Test]
    public async Task Move_With_Already_Cancelled_Token_Writes_Nothing()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await ContentEditingService.MoveAsync(child1.Key, root2.Key, Constants.Security.SuperUserKey, cts.Token));

        var refetchedChild1 = await ContentEditingService.GetAsync(child1.Key);
        Assert.IsNotNull(refetchedChild1);
        Assert.AreEqual(root1.Id, refetchedChild1!.ParentId, "An already-cancelled token must fail before any write, so the content must not have moved.");
    }
}
