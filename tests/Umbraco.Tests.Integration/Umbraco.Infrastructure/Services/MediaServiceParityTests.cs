using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Pins the media service behaviours that differ from the document service: the ones kept from the NPoco media
///     service (the sorting flag, one deleted notification per item, the key checks, deleting every surviving
///     descendant of a deleted type) and the ones adopted from the engine (statuses instead of exceptions, the bulk
///     deleting notification when emptying the bin, parent keys on re-homed children).
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaServiceParityTests : UmbracoIntegrationTest
{
    private const string FolderAlias = "parityFolder";
    private const string ImageAlias = "parityImage";

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaType _folderType = null!;
    private IMediaType _imageType = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<MediaSavingNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaDeletingNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaDeletedNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaMovingToRecycleBinNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaMovedToRecycleBinNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaMovedNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaEmptyingRecycleBinNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaEmptiedRecycleBinNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaDeletingVersionsNotification, MediaNotificationRecorder>()
        .AddNotificationHandler<MediaDeletedVersionsNotification, MediaNotificationRecorder>();

    [SetUp]
    public async Task CreateMediaTypes()
    {
        MediaNotificationRecorder.Reset();

        _folderType = MediaTypeBuilder.CreateSimpleMediaType(FolderAlias, "Parity Folder");
        await MediaTypeService.CreateAsync(_folderType, Constants.Security.SuperUserKey);

        _imageType = MediaTypeBuilder.CreateImageMediaType(ImageAlias);
        await MediaTypeService.CreateAsync(_imageType, Constants.Security.SuperUserKey);
    }

    [TearDown]
    public void ResetRecorder() => MediaNotificationRecorder.Reset();

    [Test]
    public async Task SortAsync_FlagsTheSavingNotificationAsSorting()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        IMedia first = await CreateFolderAsync("First", folder.Key);
        IMedia second = await CreateFolderAsync("Second", folder.Key);
        MediaNotificationRecorder.Reset();

        Attempt<ContentSortOperationStatus> result = await MediaService.SortAsync([second.Key, first.Key], Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        MediaSavingNotification saving = MediaNotificationRecorder.Notifications.OfType<MediaSavingNotification>().Single();
        Assert.That(saving.IsSorting, Is.True);
        Assert.That(saving.SavedEntities.Select(x => x.Key), Is.EqualTo(new[] { second.Key, first.Key }));

        IMedia[] children = [.. (await MediaService.GetChildrenAsync(folder.Key, 0, 10, null, null, CancellationToken.None)).Items];
        Assert.That(children.Select(x => x.Key), Is.EqualTo(new[] { second.Key, first.Key }));
    }

    [Test]
    public async Task SaveAsync_DoesNotFlagTheSavingNotificationAsSorting()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);

        MediaSavingNotification saving = MediaNotificationRecorder.Notifications.OfType<MediaSavingNotification>().Single();
        Assert.That(saving.IsSorting, Is.False);
        Assert.That(saving.SavedEntities.Single().Key, Is.EqualTo(folder.Key));
    }

    [Test]
    public async Task DeleteAsync_PublishesOneDeletedNotificationPerItem_DeepestFirst()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        IMedia child = await CreateFolderAsync("Child", folder.Key);
        IMedia grandchild = await CreateFolderAsync("Grandchild", child.Key);
        MediaNotificationRecorder.Reset();

        Attempt<ContentDeleteOperationStatus> result = await MediaService.DeleteAsync(folder, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        MediaDeletedNotification[] deleted = [.. MediaNotificationRecorder.Notifications.OfType<MediaDeletedNotification>()];
        Assert.That(deleted.Select(x => x.DeletedEntities.Single().Key), Is.EqualTo(new[] { grandchild.Key, child.Key, folder.Key }));
        Assert.That(await MediaService.GetByIdAsync(grandchild.Key, CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task SaveAsync_RejectsAVersion7Key_WhenTheMediaPathSchemeDoesNotSupportIt()
    {
        Assert.That(GetRequiredService<IMediaPathScheme>().SupportsGuid7, Is.False);
        var media = new Media("Version 7", Constants.System.Root, _folderType) { Key = Guid.CreateVersion7() };

        Attempt<ContentSaveOperationStatus> result = await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(ContentSaveOperationStatus.InvalidKey));
        Assert.That(media.HasIdentity, Is.False);
        Assert.That(await MediaService.GetByIdAsync(media.Key, CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task SaveAsync_RejectsANewItemWhoseKeyAlreadyExists()
    {
        IMedia existing = await CreateFolderAsync("Existing", null);
        var duplicate = new Media("Duplicate", Constants.System.Root, _folderType) { Key = existing.Key };

        Attempt<ContentSaveOperationStatus> result = await MediaService.SaveAsync(duplicate, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(ContentSaveOperationStatus.DuplicateKey));
        Assert.That(duplicate.HasIdentity, Is.False);
        Assert.That((await MediaService.GetByIdAsync(existing.Key, CancellationToken.None))!.Name, Is.EqualTo("Existing"));
    }

    [Test]
    public async Task SaveAsync_RejectsABlankName_WithAStatus()
    {
        var media = new Media(string.Empty, Constants.System.Root, _folderType);

        Attempt<ContentSaveOperationStatus> result = await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo(ContentSaveOperationStatus.InvalidName));
        Assert.That(media.HasIdentity, Is.False);
    }

    [Test]
    public async Task DeleteOfTypesAsync_MovesEveryDescendantOfAnotherType_ToTheRecycleBin_AtAnyDepth()
    {
        IMediaType survivingFolderType = MediaTypeBuilder.CreateSimpleMediaType("survivingFolder", "Surviving Folder");
        await MediaTypeService.CreateAsync(survivingFolderType, Constants.Security.SuperUserKey);

        IMedia folder = await CreateFolderAsync("Folder", null);
        IMedia deletedSubFolder = await CreateFolderAsync("Deleted sub folder", folder.Key);
        IMedia deepImage = await MediaService.CreateAndSaveAsync("Deep image", deletedSubFolder.Key, ImageAlias, Constants.Security.SuperUserKey, CancellationToken.None);
        IMedia survivingSubFolder = await MediaService.CreateAndSaveAsync("Surviving sub folder", folder.Key, survivingFolderType.Alias, Constants.Security.SuperUserKey, CancellationToken.None);
        IMedia nestedImage = await MediaService.CreateAndSaveAsync("Nested image", survivingSubFolder.Key, ImageAlias, Constants.Security.SuperUserKey, CancellationToken.None);
        MediaNotificationRecorder.Reset();

        Attempt<ContentDeleteOfTypesOperationStatus> result = await MediaService.DeleteOfTypesAsync([_folderType.Key], Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(await MediaService.GetByIdAsync(folder.Key, CancellationToken.None), Is.Null);
        Assert.That(await MediaService.GetByIdAsync(deletedSubFolder.Key, CancellationToken.None), Is.Null);

        IMedia trashedDeepImage = (await MediaService.GetByIdAsync(deepImage.Key, CancellationToken.None))!;
        Assert.That(trashedDeepImage.Trashed, Is.True);
        Assert.That(trashedDeepImage.ParentId, Is.EqualTo(Constants.System.RecycleBinMedia));

        IMedia trashedSurvivingSubFolder = (await MediaService.GetByIdAsync(survivingSubFolder.Key, CancellationToken.None))!;
        Assert.That(trashedSurvivingSubFolder.Trashed, Is.True);
        Assert.That(trashedSurvivingSubFolder.ParentId, Is.EqualTo(Constants.System.RecycleBinMedia));

        IMedia trashedNestedImage = (await MediaService.GetByIdAsync(nestedImage.Key, CancellationToken.None))!;
        Assert.That(trashedNestedImage.Trashed, Is.True);
        Assert.That(trashedNestedImage.ParentKey, Is.EqualTo(survivingSubFolder.Key));
        Assert.That(trashedNestedImage.Path, Is.EqualTo(Constants.System.RecycleBinMediaPathPrefix + survivingSubFolder.Id + "," + nestedImage.Id));

        MediaMovedToRecycleBinNotification movedToRecycleBin = MediaNotificationRecorder.Notifications.OfType<MediaMovedToRecycleBinNotification>().Single();
        Assert.That(movedToRecycleBin.MoveInfoCollection.Select(x => x.Entity.Key), Is.EquivalentTo(new[] { deepImage.Key, survivingSubFolder.Key, nestedImage.Key }));
    }

    [Test]
    public async Task DeleteOfTypesAsync_ReportsAnUnknownType()
    {
        Attempt<ContentDeleteOfTypesOperationStatus> result = await MediaService.DeleteOfTypesAsync([Guid.NewGuid()], Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo(ContentDeleteOfTypesOperationStatus.NotFound));
    }

    [Test]
    public async Task DeleteOfTypesAsync_PublishesTheDeletingNotification_WhenNoMediaOfTheTypeExists()
    {
        Attempt<ContentDeleteOfTypesOperationStatus> result = await MediaService.DeleteOfTypesAsync([_folderType.Key], Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(MediaNotificationRecorder.Notifications.OfType<MediaDeletingNotification>().Single().DeletedEntities, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task MoveAsync_CompletesItsScope_WhenTheParentIsRejected(bool parentTrashed)
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        Guid parentKey = Guid.NewGuid();
        if (parentTrashed)
        {
            IMedia trashed = await CreateFolderAsync("Trashed", null);
            await MediaService.MoveToRecycleBinAsync(trashed, Constants.Security.SuperUserKey, CancellationToken.None);
            parentKey = trashed.Key;
        }

        IMedia after;
        using (ICoreScope scope = GetRequiredService<ICoreScopeProvider>().CreateCoreScope())
        {
            Attempt<ContentMoveOperationStatus> result = await MediaService.MoveAsync(folder, parentKey, true, Constants.Security.SuperUserKey, CancellationToken.None);
            Assert.That(result.Result, Is.EqualTo(parentTrashed ? ContentMoveOperationStatus.ParentTrashed : ContentMoveOperationStatus.ParentNotFound));

            after = await CreateFolderAsync("After", null);
            scope.Complete();
        }

        Assert.That(await MediaService.GetByIdAsync(after.Key, CancellationToken.None), Is.Not.Null);
    }

    [Test]
    public async Task EmptyRecycleBinAsync_PublishesTheBulkDeletingNotification_AndACancelKeepsTheItems()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        await MediaService.MoveToRecycleBinAsync(folder, Constants.Security.SuperUserKey, CancellationToken.None);
        MediaNotificationRecorder.Reset();
        MediaNotificationRecorder.CancelWhen = notification => notification is MediaDeletingNotification;

        Attempt<ContentEmptyRecycleBinOperationStatus> cancelled = await MediaService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(cancelled.Result, Is.EqualTo(ContentEmptyRecycleBinOperationStatus.CancelledByNotification));
        Assert.That(MediaNotificationRecorder.Notifications.OfType<MediaDeletingNotification>().Single().DeletedEntities.Single().Key, Is.EqualTo(folder.Key));
        Assert.That(MediaNotificationRecorder.Notifications.OfType<MediaDeletedNotification>(), Is.Empty);
        Assert.That(await MediaService.GetByIdAsync(folder.Key, CancellationToken.None), Is.Not.Null);

        MediaNotificationRecorder.Reset();

        Attempt<ContentEmptyRecycleBinOperationStatus> emptied = await MediaService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(emptied.Success, Is.True);
        Type[] order = [.. MediaNotificationRecorder.Notifications.Select(x => x.GetType())];
        Assert.That(order, Is.EqualTo(new[] { typeof(MediaEmptyingRecycleBinNotification), typeof(MediaDeletingNotification), typeof(MediaDeletedNotification), typeof(MediaEmptiedRecycleBinNotification) }));
        Assert.That(await MediaService.GetByIdAsync(folder.Key, CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task MoveAsync_ReportsAMissingParent()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);

        Attempt<ContentMoveOperationStatus> result = await MediaService.MoveAsync(folder, Guid.NewGuid(), true, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo(ContentMoveOperationStatus.ParentNotFound));
        Assert.That((await MediaService.GetByIdAsync(folder.Key, CancellationToken.None))!.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task MoveAsync_ReportsATrashedParent()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        IMedia trashed = await CreateFolderAsync("Trashed", null);
        await MediaService.MoveToRecycleBinAsync(trashed, Constants.Security.SuperUserKey, CancellationToken.None);

        Attempt<ContentMoveOperationStatus> result = await MediaService.MoveAsync(folder, trashed.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo(ContentMoveOperationStatus.ParentTrashed));
    }

    [Test]
    public async Task MoveToRecycleBinAsync_ReportsACancelledNotification()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        MediaNotificationRecorder.CancelWhen = notification => notification is MediaMovingToRecycleBinNotification;

        Attempt<ContentMoveToRecycleBinOperationStatus> result = await MediaService.MoveToRecycleBinAsync(folder, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Result, Is.EqualTo(ContentMoveToRecycleBinOperationStatus.CancelledByNotification));
        Assert.That((await MediaService.GetByIdAsync(folder.Key, CancellationToken.None))!.Trashed, Is.False);
    }

    [Test]
    public async Task MoveAsync_RestoringWithoutDescendants_ReHomesTheChildrenUnderTheRecycleBin_WithTheirParentKey()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        IMedia child = await CreateFolderAsync("Child", folder.Key);
        IMedia grandchild = await CreateFolderAsync("Grandchild", child.Key);
        await MediaService.MoveToRecycleBinAsync(folder, Constants.Security.SuperUserKey, CancellationToken.None);
        IMedia trashedFolder = (await MediaService.GetByIdAsync(folder.Key, CancellationToken.None))!;

        Attempt<ContentMoveOperationStatus> result = await MediaService.MoveAsync(trashedFolder, null, false, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        IMedia restored = (await MediaService.GetByIdAsync(folder.Key, CancellationToken.None))!;
        Assert.That(restored.Trashed, Is.False);
        Assert.That(restored.ParentId, Is.EqualTo(Constants.System.Root));

        // the moved notification carries the instances the service mutated, so the parent key is the one the
        // service set rather than the one a fresh read resolves from the parent node
        MediaMovedNotification moved = MediaNotificationRecorder.Notifications.OfType<MediaMovedNotification>().Single();
        IMedia movedChild = moved.MoveInfoCollection.Single(x => x.Entity.Key == child.Key).Entity;
        Assert.That(movedChild.ParentKey, Is.EqualTo(Constants.System.RecycleBinMediaKey));

        IMedia reHomedChild = (await MediaService.GetByIdAsync(child.Key, CancellationToken.None))!;
        Assert.That(reHomedChild.Trashed, Is.True);
        Assert.That(reHomedChild.ParentId, Is.EqualTo(Constants.System.RecycleBinMedia));
        Assert.That(reHomedChild.ParentKey, Is.EqualTo(Constants.System.RecycleBinMediaKey));
        Assert.That(reHomedChild.Path, Is.EqualTo(Constants.System.RecycleBinMediaPathPrefix + child.Id));

        IMedia keptGrandchild = (await MediaService.GetByIdAsync(grandchild.Key, CancellationToken.None))!;
        Assert.That(keptGrandchild.Trashed, Is.True);
        Assert.That(keptGrandchild.ParentKey, Is.EqualTo(child.Key));
        Assert.That(keptGrandchild.Path, Is.EqualTo(reHomedChild.Path + "," + grandchild.Id));
    }

    [Test]
    public async Task DeleteVersionAsync_PublishesTheVersionNotifications_AndKeepsTheCurrentVersion()
    {
        IMedia folder = await CreateFolderAsync("Folder", null);
        MediaNotificationRecorder.Reset();

        Attempt<ContentVersionOperationStatus> result = await MediaService.DeleteVersionAsync(folder.Key, folder.VersionId, false, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(MediaNotificationRecorder.Notifications.OfType<MediaDeletingVersionsNotification>().Single().Key, Is.EqualTo(folder.Key));
        Assert.That(MediaNotificationRecorder.Notifications.OfType<MediaDeletedVersionsNotification>().Single().Key, Is.EqualTo(folder.Key));
        Assert.That((await MediaService.GetByIdAsync(folder.Key, CancellationToken.None))!.VersionId, Is.EqualTo(folder.VersionId));
    }

    private Task<IMedia> CreateFolderAsync(string name, Guid? parentKey)
        => MediaService.CreateAndSaveAsync(name, parentKey, FolderAlias, Constants.Security.SuperUserKey, CancellationToken.None);

    private sealed class MediaNotificationRecorder :
        INotificationHandler<MediaSavingNotification>,
        INotificationHandler<MediaDeletingNotification>,
        INotificationHandler<MediaDeletedNotification>,
        INotificationHandler<MediaMovingToRecycleBinNotification>,
        INotificationHandler<MediaMovedToRecycleBinNotification>,
        INotificationHandler<MediaMovedNotification>,
        INotificationHandler<MediaEmptyingRecycleBinNotification>,
        INotificationHandler<MediaEmptiedRecycleBinNotification>,
        INotificationHandler<MediaDeletingVersionsNotification>,
        INotificationHandler<MediaDeletedVersionsNotification>
    {
        public static List<INotification> Notifications { get; } = [];

        public static Func<INotification, bool>? CancelWhen { get; set; }

        public static void Reset()
        {
            Notifications.Clear();
            CancelWhen = null;
        }

        public void Handle(MediaSavingNotification notification) => Record(notification);

        public void Handle(MediaDeletingNotification notification) => Record(notification);

        public void Handle(MediaDeletedNotification notification) => Record(notification);

        public void Handle(MediaMovingToRecycleBinNotification notification) => Record(notification);

        public void Handle(MediaMovedToRecycleBinNotification notification) => Record(notification);

        public void Handle(MediaMovedNotification notification) => Record(notification);

        public void Handle(MediaEmptyingRecycleBinNotification notification) => Record(notification);

        public void Handle(MediaEmptiedRecycleBinNotification notification) => Record(notification);

        public void Handle(MediaDeletingVersionsNotification notification) => Record(notification);

        public void Handle(MediaDeletedVersionsNotification notification) => Record(notification);

        private static void Record(INotification notification)
        {
            Notifications.Add(notification);
            if (notification is ICancelableNotification cancelable && CancelWhen?.Invoke(notification) is true)
            {
                cancelable.Cancel = true;
            }
        }
    }
}
