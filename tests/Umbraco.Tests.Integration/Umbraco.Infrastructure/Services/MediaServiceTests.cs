// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed class MediaServiceTests : UmbracoIntegrationTest
{
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Sort_Preserves_Property_Data_When_Items_Loaded_Without_It(bool useSortChildren)
    {
        var mediaType = await CreateMediaType();

        var keys = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "Media " + i, -1);
            media.SetValue("title", "Title " + i);
            await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
            keys.Add(media.Key);
        }

        // Rotate so every item's sort order changes (and is therefore re-saved).
        Guid[] rotated = [keys[1], keys[2], keys[0]];
        if (useSortChildren)
        {
            var result = await MediaService.SortChildrenAsync(null, rotated, Constants.Security.SuperUserKey, CancellationToken.None);
            Assert.That(result.Success, Is.True);
        }
        else
        {
            var result = await MediaService.SortAsync(rotated, Constants.Security.SuperUserKey, CancellationToken.None);
            Assert.That(result.Success, Is.True);
        }

        IMedia first = (await MediaService.GetByIdAsync(keys[0], CancellationToken.None))!;
        IMedia second = (await MediaService.GetByIdAsync(keys[1], CancellationToken.None))!;
        IMedia third = (await MediaService.GetByIdAsync(keys[2], CancellationToken.None))!;

        // Every sorted item must retain its property data, and the requested order must be applied.
        Assert.Multiple(() =>
        {
            Assert.That(first.GetValue<string>("title"), Is.EqualTo("Title 0"));
            Assert.That(second.GetValue<string>("title"), Is.EqualTo("Title 1"));
            Assert.That(third.GetValue<string>("title"), Is.EqualTo("Title 2"));
            Assert.That(second.SortOrder, Is.EqualTo(0));
            Assert.That(third.SortOrder, Is.EqualTo(1));
            Assert.That(first.SortOrder, Is.EqualTo(2));
        });
    }

    public static void ConfigureSortingNotificationRecording(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<SortingNotificationRecorder>();
        builder.AddNotificationHandler<MediaSavingNotification, SortingRecordingMediaSavingHandler>();
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureSortingNotificationRecording))]
    public async Task Sort_Publishes_Saving_Notification_Flagged_As_Sorting()
    {
        var mediaType = await CreateMediaType();
        IMedia first = MediaBuilder.CreateSimpleMedia(mediaType, "First", -1);
        IMedia second = MediaBuilder.CreateSimpleMedia(mediaType, "Second", -1);
        await MediaService.SaveAsync(first, Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.SaveAsync(second, Constants.Security.SuperUserKey, CancellationToken.None);

        var recorder = GetRequiredService<SortingNotificationRecorder>();
        Assert.That(recorder.IsSortingValues, Is.EqualTo(new[] { false, false }));
        recorder.IsSortingValues.Clear();

        var result = await MediaService.SortAsync([second.Key, first.Key], Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        IMedia reloadedSecond = (await MediaService.GetByIdAsync(second.Key, CancellationToken.None))!;
        IMedia reloadedFirst = (await MediaService.GetByIdAsync(first.Key, CancellationToken.None))!;

        Assert.Multiple(() =>
        {
            Assert.That(recorder.IsSortingValues, Is.EqualTo(new[] { true }));
            Assert.That(reloadedSecond.SortOrder, Is.EqualTo(0));
            Assert.That(reloadedFirst.SortOrder, Is.EqualTo(1));
        });
    }

    internal sealed class SortingNotificationRecorder
    {
        public List<bool> IsSortingValues { get; } = new();
    }

    internal sealed class SortingRecordingMediaSavingHandler(SortingNotificationRecorder recorder)
        : INotificationHandler<MediaSavingNotification>
    {
        public void Handle(MediaSavingNotification notification) => recorder.IsSortingValues.Add(notification.IsSorting);
    }

    [Test]
    public async Task Can_Create_Media()
    {
        var mediaType = await CreateMediaType();

        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "Media", -1);
        media.SetValue("title", "Title");
        media.SetValue("bodyText", "Body text");
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        media = await MediaService.GetByIdAsync(media.Key, CancellationToken.None);

        Assert.IsNotNull(media);
        Assert.AreEqual("Title", media.GetValue("title"));
        Assert.AreEqual("Body text", media.GetValue("bodyText"));
    }

    [Test]
    public async Task Cannot_Create_Media_With_Guid_7_When_Default_Media_Path_Scheme_Is_Registered()
    {
        var mediaType = await CreateMediaType();

        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "Media", -1);
        media.Key = Guid.CreateVersion7();
        var mediaSaveAttempt = await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(mediaSaveAttempt.Success);
        Assert.AreEqual(ContentSaveOperationStatus.InvalidKey, mediaSaveAttempt.Result);
    }

    private async Task<IMediaType> CreateMediaType()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test");
        var createMediaTypeResult = await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        Assert.IsTrue(createMediaTypeResult.Success);
        return mediaType;
    }

    [Test]
    public async Task Can_Update_Media_Property_Values()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "hello", -1);
        media.SetValue("title", "title of mine");
        media.SetValue("bodyText", "hello world");
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        // re-get
        media = await MediaService.GetByIdAsync(media.Key, CancellationToken.None);
        media.SetValue("title", "another title of mine"); // Change a value
        media.SetValue("bodyText", null); // Clear a value
        media.SetValue("author", "new author"); // Add a value
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        // re-get
        media = await MediaService.GetByIdAsync(media.Key, CancellationToken.None);
        Assert.AreEqual("another title of mine", media.GetValue("title"));
        Assert.IsNull(media.GetValue("bodyText"));
        Assert.AreEqual("new author", media.GetValue("author"));
    }

    /// <summary>
    ///     Used to list out all ambiguous events that will require dispatching with a name
    /// </summary>
    [Test]
    [Explicit]
    public void List_Ambiguous_Events()
    {
        var events = MediaService.GetType().GetEvents(BindingFlags.Static | BindingFlags.Public);
        var typedEventHandler = typeof(TypedEventHandler<,>);
        foreach (var e in events)
        {
            // only continue if this is a TypedEventHandler
            if (!e.EventHandlerType.IsGenericType)
            {
                continue;
            }

            var typeDef = e.EventHandlerType.GetGenericTypeDefinition();
            if (typedEventHandler != typeDef)
            {
                continue;
            }

            // get the event arg type
            var eventArgType = e.EventHandlerType.GenericTypeArguments[1];

            var found = EventNameExtractor.FindEvent(
                typeof(MediaService),
                eventArgType,
                EventNameExtractor.MatchIngNames);
            if (!found.Success && found.Result.Error == EventNameExtractorError.Ambiguous)
            {
                Console.WriteLine($"Ambiguous event, source: {typeof(MediaService)}, args: {eventArgType}");
            }
        }
    }

    [Test]
    public async Task Get_Paged_Children_With_Media_Type_Filter()
    {
        var mediaType1 = MediaTypeBuilder.CreateImageMediaType("Image2");
        await MediaTypeService.CreateAsync(mediaType1, Constants.Security.SuperUserKey);
        var mediaType2 = MediaTypeBuilder.CreateImageMediaType("Image3");
        await MediaTypeService.CreateAsync(mediaType2, Constants.Security.SuperUserKey);

        for (var i = 0; i < 10; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(mediaType1, -1);
            await MediaService.SaveAsync(m1, Constants.Security.SuperUserKey, CancellationToken.None);
            var m2 = MediaBuilder.CreateMediaImage(mediaType2, -1);
            await MediaService.SaveAsync(m2, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        Guid[] mediaTypeKeys = [mediaType1.Key, mediaType2.Key];

        var result = await MediaService.GetPagedOfTypesAsync(mediaTypeKeys, 0, 11, Ordering.By("sortOrder"), CancellationToken.None);
        Assert.AreEqual(11, result.Items.Count());
        Assert.AreEqual(20, result.Total);

        result = await MediaService.GetPagedOfTypesAsync(mediaTypeKeys, 11, 11, Ordering.By("sortOrder"), CancellationToken.None);
        Assert.AreEqual(9, result.Items.Count());
        Assert.AreEqual(20, result.Total);
    }

    [Test]
    public async Task Can_Move_Media()
    {
        var mediaItems = await CreateTrashedTestMedia();
        var media = await MediaService.GetByIdAsync(mediaItems.Item3.Key, CancellationToken.None);

        var result = await MediaService.MoveAsync(media, mediaItems.Item2.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item2.Id));
        Assert.That(media.Trashed, Is.False);
    }

    [Test]
    public async Task Can_Move_Media_Populates_ParentKey()
    {
        var mediaItems = await CreateTrashedTestMedia();
        var media = await MediaService.GetByIdAsync(mediaItems.Item3.Key, CancellationToken.None);

        var result = await MediaService.MoveAsync(media, mediaItems.Item2.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(media.ParentKey, Is.EqualTo(mediaItems.Item2.Key));
    }

    [Test]
    public async Task Can_Move_Media_To_RecycleBin()
    {
        var mediaItems = await CreateTrashedTestMedia();
        var media = await MediaService.GetByIdAsync(mediaItems.Item1.Key, CancellationToken.None);

        var result = await MediaService.MoveToRecycleBinAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(media.ParentId, Is.EqualTo(Constants.System.RecycleBinMedia));
        Assert.That(media.Trashed, Is.True);
    }

    [Test]
    public async Task Can_Move_Media_From_RecycleBin()
    {
        var mediaItems = await CreateTrashedTestMedia();
        var media = await MediaService.GetByIdAsync(mediaItems.Item4.Key, CancellationToken.None);

        var result = await MediaService.MoveAsync(media, mediaItems.Item1.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);
        var mediaChild = await MediaService.GetByIdAsync(mediaItems.Item5.Key, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item1.Id));
        Assert.That(media.Trashed, Is.False);
        Assert.That(mediaChild.ParentId, Is.EqualTo(mediaItems.Item4.Id));
        Assert.That(mediaChild.Trashed, Is.False);
    }

    [Test]
    public async Task Cannot_Save_Media_With_Empty_Name()
    {
        var mediaType = MediaTypeBuilder.CreateNewMediaType();
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        var media = await MediaService.CreateAsync(string.Empty, (Guid?)null, Constants.Conventions.MediaTypes.VideoAlias, Constants.Security.SuperUserKey, CancellationToken.None);

        var result = await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(ContentSaveOperationStatus.InvalidName));
    }

    [Test]
    public async Task Can_Get_Media_By_Path()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaBuilder.CreateMediaImage(mediaType, -1);
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        var mediaPath = "/media/test-image.png";
        var resolvedMedia = await MediaService.GetMediaByPathAsync(mediaPath, CancellationToken.None);

        Assert.IsNotNull(resolvedMedia);
        Assert.That(resolvedMedia.GetValue(Constants.Conventions.Media.File).ToString() == mediaPath);
    }

    [Test]
    public async Task Can_Get_Media_With_Crop_By_Path()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaTypeWithCrop("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaBuilder.CreateMediaImageWithCrop(mediaType, -1);
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);

        var mediaPath = "/media/test-image.png";
        var resolvedMedia = await MediaService.GetMediaByPathAsync(mediaPath, CancellationToken.None);

        Assert.IsNotNull(resolvedMedia);
        Assert.That(resolvedMedia.GetValue(Constants.Conventions.Media.File).ToString().Contains(mediaPath));
    }

    [Test]
    public async Task Can_Get_Paged_Children()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        for (var i = 0; i < 10; i++)
        {
            var c1 = MediaBuilder.CreateMediaImage(mediaType, -1);
            await MediaService.SaveAsync(c1, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        var service = MediaService;

        var page = await service.GetChildrenAsync(null, 0, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await service.GetChildrenAsync(null, 6, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));
    }

    [Test]
    public async Task Can_Get_Paged_Children_Dont_Get_Descendants()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        // Only add 9 as we also add a folder with children.
        for (var i = 0; i < 9; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(mediaType, -1);
            await MediaService.SaveAsync(m1, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        var mediaTypeForFolder = MediaTypeBuilder.CreateImageMediaType("Folder2");
        await MediaTypeService.CreateAsync(mediaTypeForFolder, Constants.Security.SuperUserKey);
        var mediaFolder = MediaBuilder.CreateMediaFolder(mediaTypeForFolder, -1);
        await MediaService.SaveAsync(mediaFolder, Constants.Security.SuperUserKey, CancellationToken.None);
        for (var i = 0; i < 10; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(mediaType, mediaFolder.Id);
            await MediaService.SaveAsync(m1, Constants.Security.SuperUserKey, CancellationToken.None);
        }

        var service = MediaService;

        // Children in root including the folder - not the descendants in the folder.
        var page = await service.GetChildrenAsync(null, 0, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await service.GetChildrenAsync(null, 6, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));

        // Children in folder.
        page = await service.GetChildrenAsync(mediaFolder.Key, 0, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(6));
        Assert.That(page.Total, Is.EqualTo(10));
        page = await service.GetChildrenAsync(mediaFolder.Key, 6, 6, null, null, CancellationToken.None);
        Assert.That(page.Items.Count(), Is.EqualTo(4));
        Assert.That(page.Total, Is.EqualTo(10));
    }

    private async Task<Tuple<IMedia, IMedia, IMedia, IMedia, IMedia>> CreateTrashedTestMedia()
    {
        var folderMediaType = await MediaTypeService.GetAsync(Constants.Conventions.MediaTypes.Folder);
        var folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
        await MediaService.SaveAsync(folder, Constants.Security.SuperUserKey, CancellationToken.None);

        var folder2 = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
        await MediaService.SaveAsync(folder2, Constants.Security.SuperUserKey, CancellationToken.None);

        var imageMediaType = await MediaTypeService.GetAsync(Constants.Conventions.MediaTypes.Image);
        var image = MediaBuilder.CreateMediaImage(imageMediaType, folder.Id);
        await MediaService.SaveAsync(image, Constants.Security.SuperUserKey, CancellationToken.None);

        var folderTrashed = MediaBuilder.CreateMediaFolder(folderMediaType, Constants.System.RecycleBinMedia);
        folderTrashed.Trashed = true;
        await MediaService.SaveAsync(folderTrashed, Constants.Security.SuperUserKey, CancellationToken.None);

        var imageTrashed = MediaBuilder.CreateMediaImage(imageMediaType, folderTrashed.Id);
        imageTrashed.Trashed = true;
        await MediaService.SaveAsync(imageTrashed, Constants.Security.SuperUserKey, CancellationToken.None);

        return new Tuple<IMedia, IMedia, IMedia, IMedia, IMedia>(folder, folder2, image, folderTrashed, imageTrashed);
    }

    #region Concurrency Tests

    public static void ConfigureConcurrencyTest(IUmbracoBuilder builder) =>
        builder.AddNotificationAsyncHandler<MediaSavingNotification, ReadLockAcquiringMediaSavingHandler>();

    /// <summary>
    /// Verifies that parallel media saves don't deadlock when a notification handler acquires a read lock.
    /// </summary>
    /// <remarks>
    /// Before the fix (issue #21125), this test would deadlock because:
    /// 1. Thread A publishes MediaSavingNotification, handler calls GetByIdAsync (acquires read lock).
    /// 2. Thread B publishes MediaSavingNotification, handler calls GetByIdAsync (acquires read lock).
    /// 3. Thread A tries to acquire write lock - blocked waiting for Thread B's read lock.
    /// 4. Thread B tries to acquire write lock - blocked waiting for Thread A's read lock.
    /// = Deadlock
    /// After the fix, write locks are acquired before publishing notifications, so the deadlock cannot occur.
    /// </remarks>
    [Test]
    [Timeout(10000)]
    [ConfigureBuilder(ActionName = nameof(ConfigureConcurrencyTest))]
    public async Task Parallel_Media_Save_Does_Not_Deadlock_When_Notification_Handler_Acquires_Read_Lock()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        const int numberOfMediaItems = 5;
        var mediaItems = new List<IMedia>();

        // Create media items first so they have an identity and will be handled by the notification handler.
        for (var i = 0; i < numberOfMediaItems; i++)
        {
            var media = MediaBuilder.CreateSimpleMedia(mediaType, $"Test Media {i}", Constants.System.Root);
            await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
            mediaItems.Add(media);
        }

        var exceptions = new List<Exception>();
        var lockObj = new Lock();

        var tasks = mediaItems.Select(media => RunWithSuppressedExecutionContext(async () =>
        {
            try
            {
                media.Name += " Updated";
                await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToList();

        await Task.WhenAll(tasks);

        Assert.IsEmpty(
            exceptions,
            $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");

        // Verify all media items were updated successfully
        foreach (var media in mediaItems)
        {
            var retrieved = await MediaService.GetByIdAsync(media.Key, CancellationToken.None);
            Assert.That(retrieved, Is.Not.Null, $"Media '{media.Name}' should be retrievable after save");
            Assert.That(retrieved!.Name, Does.EndWith("Updated"), $"Media should have been updated");
        }
    }

    /// <summary>
    /// Verifies that parallel media deletes don't deadlock when a notification handler is registered.
    /// </summary>
    [Test]
    [Timeout(10000)]
    [ConfigureBuilder(ActionName = nameof(ConfigureConcurrencyTest))]
    public async Task Parallel_Media_Delete_Does_Not_Deadlock()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        const int numberOfMediaItems = 5;
        var mediaItems = new List<IMedia>();

        for (var i = 0; i < numberOfMediaItems; i++)
        {
            var media = MediaBuilder.CreateSimpleMedia(mediaType, $"Test Media {i}", Constants.System.Root);
            await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
            mediaItems.Add(media);
        }

        var exceptions = new List<Exception>();
        var lockObj = new Lock();

        var tasks = mediaItems.Select(media => RunWithSuppressedExecutionContext(async () =>
        {
            try
            {
                await MediaService.DeleteAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToList();

        await Task.WhenAll(tasks);

        Assert.IsEmpty(
            exceptions,
            $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");

        // Verify all media items were deleted
        foreach (var media in mediaItems)
        {
            var retrieved = await MediaService.GetByIdAsync(media.Key, CancellationToken.None);
            Assert.That(retrieved, Is.Null, $"Media '{media.Name}' should have been deleted");
        }
    }

    private static Task RunWithSuppressedExecutionContext(Func<Task> action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(action);
        }
    }

    /// <summary>
    /// A notification handler that acquires a read lock by calling MediaService.GetByIdAsync.
    /// This simulates real-world scenarios where handlers need to read related data.
    /// </summary>
    /// <remarks>
    /// Before the fix for issue #21125, this handler would cause deadlocks when multiple
    /// media items are saved in parallel because:
    /// 1. The notification is published BEFORE the write lock is acquired
    /// 2. This handler calls GetByIdAsync which acquires a read lock
    /// 3. Multiple threads each hold read locks and then try to upgrade to write locks
    /// 4. SQL Server detects this as a deadlock; SQLite hangs indefinitely
    /// </remarks>
    internal sealed class ReadLockAcquiringMediaSavingHandler : INotificationAsyncHandler<MediaSavingNotification>
    {
        private readonly IMediaService _mediaService;

        public ReadLockAcquiringMediaSavingHandler(IMediaService mediaService) =>
            _mediaService = mediaService;

        public async Task HandleAsync(MediaSavingNotification notification, CancellationToken cancellationToken)
        {
            foreach (var media in notification.SavedEntities)
            {
                // This call acquires a read lock on MediaTree.
                // Before the fix, this could cause deadlocks when combined with parallel saves.
                if (media.HasIdentity)
                {
                    await _mediaService.GetByIdAsync(media.Key, cancellationToken);
                }
            }
        }
    }

    #endregion

    [Test]
    public async Task SortChildren_Persists_The_Supplied_Order()
    {
        var folderMediaType = (await MediaTypeService.GetAsync(Constants.Conventions.MediaTypes.Folder))!;

        var root = MediaBuilder.CreateMediaFolder(folderMediaType, Constants.System.Root);
        root.Name = "Root";
        await MediaService.SaveAsync(root, Constants.Security.SuperUserKey, CancellationToken.None);

        var childKeys = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            var child = MediaBuilder.CreateMediaFolder(folderMediaType, root.Id);
            child.Name = $"Folder {i}";
            await MediaService.SaveAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
            childKeys.Add(child.Key);
        }

        async Task<Guid[]> ChildKeysInSortOrder() => (await MediaService
            .GetChildrenAsync(root.Key, 0, 100, null, null, CancellationToken.None))
            .Items
            .OrderBy(child => child.SortOrder)
            .Select(child => child.Key)
            .ToArray();

        // Children were created in ascending sort order.
        Assert.AreEqual(childKeys.ToArray(), await ChildKeysInSortOrder());

        var reversed = Enumerable.Reverse(childKeys).ToArray();
        var result = await MediaService.SortChildrenAsync(root.Key, reversed, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(reversed, await ChildKeysInSortOrder());
    }

    [Test]
    public async Task Can_Make_Duplicate_Sibling_Names_Unique()
    {
        var mediaType = await CreateMediaType();

        var first = await SaveMedia(mediaType, "image");
        var second = await SaveMedia(mediaType, "image");
        var third = await SaveMedia(mediaType, "image");

        Assert.Multiple(() =>
        {
            Assert.That(first.Name, Is.EqualTo("image"));
            Assert.That(second.Name, Is.EqualTo("image (1)"));
            Assert.That(third.Name, Is.EqualTo("image (2)"));
        });
    }

    [Test]
    public async Task Can_Make_Duplicate_Sibling_Names_Unique_Ignoring_Case()
    {
        var mediaType = await CreateMediaType();

        var first = await SaveMedia(mediaType, "Image");
        var second = await SaveMedia(mediaType, "image");

        Assert.Multiple(() =>
        {
            Assert.That(first.Name, Is.EqualTo("Image"));
            Assert.That(second.Name, Is.EqualTo("image (1)"));
        });
    }

    [Test]
    public async Task Can_Make_Name_Unique_Without_Being_Confused_By_Similar_Prefixes()
    {
        var mediaType = await CreateMediaType();

        // Decoys that share a prefix with "image" but are not the same name; these must not
        // consume a suffix nor otherwise perturb the result.
        await SaveMedia(mediaType, "image");
        await SaveMedia(mediaType, "imagery");
        await SaveMedia(mediaType, "image detail");

        var duplicate = await SaveMedia(mediaType, "image");

        Assert.That(duplicate.Name, Is.EqualTo("image (1)"));
    }

    [TestCase("50%")]
    [TestCase("a_b")]
    [TestCase("x[y")]
    public async Task Can_Make_Name_With_Like_Metacharacters_Unique(string name)
    {
        var mediaType = await CreateMediaType();

        var first = await SaveMedia(mediaType, name);
        var second = await SaveMedia(mediaType, name);

        Assert.Multiple(() =>
        {
            Assert.That(first.Name, Is.EqualTo(name));
            Assert.That(second.Name, Is.EqualTo($"{name} (1)"));
        });
    }

    [Test]
    public async Task Can_Reuse_Freed_Suffix_When_Making_Name_Unique()
    {
        var mediaType = await CreateMediaType();

        await SaveMedia(mediaType, "image");
        var one = await SaveMedia(mediaType, "image"); // image (1)
        await SaveMedia(mediaType, "image");           // image (2)

        await MediaService.DeleteAsync(one, Constants.Security.SuperUserKey, CancellationToken.None); // frees "image (1)"

        var reused = await SaveMedia(mediaType, "image");

        Assert.That(reused.Name, Is.EqualTo("image (1)"));
    }

    private async Task<IMedia> SaveMedia(IMediaType mediaType, string name)
    {
        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, name, Constants.System.Root);
        await MediaService.SaveAsync(media, Constants.Security.SuperUserKey, CancellationToken.None);
        return media;
    }
}
