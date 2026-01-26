// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
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

    [Test]
    public async Task Can_Update_Media_Property_Values()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "hello", -1);
        media.SetValue("title", "title of mine");
        media.SetValue("bodyText", "hello world");
        MediaService.Save(media);

        // re-get
        media = MediaService.GetById(media.Id);
        media.SetValue("title", "another title of mine"); // Change a value
        media.SetValue("bodyText", null); // Clear a value
        media.SetValue("author", "new author"); // Add a value
        MediaService.Save(media);

        // re-get
        media = MediaService.GetById(media.Id);
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
            MediaService.Save(m1);
            var m2 = MediaBuilder.CreateMediaImage(mediaType2, -1);
            MediaService.Save(m2);
        }

        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var filter = provider.CreateQuery<IMedia>()
                .Where(x => new List<int> { mediaType1.Id, mediaType2.Id }.Contains(x.ContentTypeId));

            var result = MediaService.GetPagedChildren(
                -1,
                0,
                11,
                out var total,
                filter,
                Ordering.By("SortOrder"));
            Assert.AreEqual(11, result.Count());
            Assert.AreEqual(20, total);

            result = MediaService.GetPagedChildren(
                -1,
                1,
                11,
                out total,
                filter,
                Ordering.By("SortOrder"));
            Assert.AreEqual(9, result.Count());
            Assert.AreEqual(20, total);
        }
    }

    [Test]
    public void Can_Move_Media()
    {
        // Arrange
        var mediaItems = CreateTrashedTestMedia();
        var media = MediaService.GetById(mediaItems.Item3.Id);

        // Act
        MediaService.Move(media, mediaItems.Item2.Id);

        // Assert
        Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item2.Id));
        Assert.That(media.Trashed, Is.False);
    }

    [Test]
    public void Can_Move_Media_To_RecycleBin()
    {
        // Arrange
        var mediaItems = CreateTrashedTestMedia();
        var media = MediaService.GetById(mediaItems.Item1.Id);

        // Act
        MediaService.MoveToRecycleBin(media);

        // Assert
        Assert.That(media.ParentId, Is.EqualTo(-21));
        Assert.That(media.Trashed, Is.True);
    }

    [Test]
    public void Can_Move_Media_From_RecycleBin()
    {
        // Arrange
        var mediaItems = CreateTrashedTestMedia();
        var media = MediaService.GetById(mediaItems.Item4.Id);

        // Act - moving out of recycle bin
        MediaService.Move(media, mediaItems.Item1.Id);
        var mediaChild = MediaService.GetById(mediaItems.Item5.Id);

        // Assert
        Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item1.Id));
        Assert.That(media.Trashed, Is.False);
        Assert.That(mediaChild.ParentId, Is.EqualTo(mediaItems.Item4.Id));
        Assert.That(mediaChild.Trashed, Is.False);
    }

    [Test]
    public async Task Cannot_Save_Media_With_Empty_Name()
    {
        // Arrange
        var mediaType = MediaTypeBuilder.CreateNewMediaType();
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        var media = MediaService.CreateMedia(string.Empty, -1, Constants.Conventions.MediaTypes.VideoAlias);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MediaService.Save(media));
    }

    [Test]
    public async Task Can_Get_Media_By_Path()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaBuilder.CreateMediaImage(mediaType, -1);
        MediaService.Save(media);

        var mediaPath = "/media/test-image.png";
        var resolvedMedia = MediaService.GetMediaByPath(mediaPath);

        Assert.IsNotNull(resolvedMedia);
        Assert.That(resolvedMedia.GetValue(Constants.Conventions.Media.File).ToString() == mediaPath);
    }

    [Test]
    public async Task Can_Get_Media_With_Crop_By_Path()
    {
        var mediaType = MediaTypeBuilder.CreateImageMediaTypeWithCrop("Image2");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaBuilder.CreateMediaImageWithCrop(mediaType, -1);
        MediaService.Save(media);

        var mediaPath = "/media/test-image.png";
        var resolvedMedia = MediaService.GetMediaByPath(mediaPath);

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
            MediaService.Save(c1);
        }

        var service = MediaService;

        var entities = service.GetPagedChildren(-1, 0, 6, out var total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = service.GetPagedChildren(-1, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));
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
            MediaService.Save(m1);
        }

        var mediaTypeForFolder = MediaTypeBuilder.CreateImageMediaType("Folder2");
        await MediaTypeService.CreateAsync(mediaTypeForFolder, Constants.Security.SuperUserKey);
        var mediaFolder = MediaBuilder.CreateMediaFolder(mediaTypeForFolder, -1);
        MediaService.Save(mediaFolder);
        for (var i = 0; i < 10; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(mediaType, mediaFolder.Id);
            MediaService.Save(m1);
        }

        var service = MediaService;

        // Children in root including the folder - not the descendants in the folder.
        var entities = service.GetPagedChildren(-1, 0, 6, out var total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = service.GetPagedChildren(-1, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));

        // Children in folder.
        entities = service.GetPagedChildren(mediaFolder.Id, 0, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(6));
        Assert.That(total, Is.EqualTo(10));
        entities = service.GetPagedChildren(mediaFolder.Id, 1, 6, out total).ToArray();
        Assert.That(entities.Length, Is.EqualTo(4));
        Assert.That(total, Is.EqualTo(10));
    }

    private Tuple<IMedia, IMedia, IMedia, IMedia, IMedia> CreateTrashedTestMedia()
    {
        // Create and Save folder-Media -> 1050
        var folderMediaType = MediaTypeService.Get(1031);
        var folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
        MediaService.Save(folder);

        // Create and Save folder-Media -> 1051
        var folder2 = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
        MediaService.Save(folder2);

        // Create and Save image-Media  -> 1052
        var imageMediaType = MediaTypeService.Get(1032);
        var image = MediaBuilder.CreateMediaImage(imageMediaType, 1050);
        MediaService.Save(image);

        // Create and Save folder-Media that is trashed -> 1053
        var folderTrashed = MediaBuilder.CreateMediaFolder(folderMediaType, -21);
        folderTrashed.Trashed = true;
        MediaService.Save(folderTrashed);

        // Create and Save image-Media child of folderTrashed -> 1054
        var imageTrashed = MediaBuilder.CreateMediaImage(imageMediaType, folderTrashed.Id);
        imageTrashed.Trashed = true;
        MediaService.Save(imageTrashed);

        return new Tuple<IMedia, IMedia, IMedia, IMedia, IMedia>(folder, folder2, image, folderTrashed, imageTrashed);
    }

    #region Concurrency Tests

    public static void ConfigureConcurrencyTest(IUmbracoBuilder builder) =>
        builder.AddNotificationHandler<MediaSavingNotification, ReadLockAcquiringMediaSavingHandler>();

    /// <summary>
    /// Verifies that parallel media saves don't deadlock when a notification handler acquires a read lock.
    /// </summary>
    /// <remarks>
    /// Before the fix (issue #21125), this test would deadlock because:
    /// 1. Thread A publishes MediaSavingNotification, handler calls GetById (acquires read lock).
    /// 2. Thread B publishes MediaSavingNotification, handler calls GetById (acquires read lock).
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
        // Arrange
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        const int numberOfMediaItems = 5;
        var mediaItems = new List<IMedia>();

        // Create media items first so they have an identity and will be handled by the notification handler.
        for (var i = 0; i < numberOfMediaItems; i++)
        {
            var media = MediaBuilder.CreateSimpleMedia(mediaType, $"Test Media {i}", Constants.System.Root);
            MediaService.Save(media);
            mediaItems.Add(media);
        }

        var exceptions = new List<Exception>();
        var lockObj = new Lock();

        // Act - update all media items in parallel.
        var tasks = mediaItems.Select(media => RunWithSuppressedExecutionContext(() =>
        {
            try
            {
                media.Name += " Updated";
                MediaService.Save(media);
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }

            return Task.CompletedTask;
        })).ToList();

        await Task.WhenAll(tasks);

        // Assert
        Assert.IsEmpty(
            exceptions,
            $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");

        // Verify all media items were updated successfully
        foreach (var media in mediaItems)
        {
            var retrieved = MediaService.GetById(media.Id);
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
        // Arrange
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        const int numberOfMediaItems = 5;
        var mediaItems = new List<IMedia>();

        // Create media items.
        for (var i = 0; i < numberOfMediaItems; i++)
        {
            var media = MediaBuilder.CreateSimpleMedia(mediaType, $"Test Media {i}", Constants.System.Root);
            MediaService.Save(media);
            mediaItems.Add(media);
        }

        var exceptions = new List<Exception>();
        var lockObj = new Lock();

        // Act - delete all media items in parallel.
        var tasks = mediaItems.Select(media => RunWithSuppressedExecutionContext(() =>
        {
            try
            {
                MediaService.Delete(media);
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }

            return Task.CompletedTask;
        })).ToList();

        await Task.WhenAll(tasks);

        // Assert
        Assert.IsEmpty(
            exceptions,
            $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");

        // Verify all media items were deleted
        foreach (var media in mediaItems)
        {
            var retrieved = MediaService.GetById(media.Id);
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
    /// A notification handler that acquires a read lock by calling MediaService.GetById.
    /// This simulates real-world scenarios where handlers need to read related data.
    /// </summary>
    /// <remarks>
    /// Before the fix for issue #21125, this handler would cause deadlocks when multiple
    /// media items are saved in parallel because:
    /// 1. The notification is published BEFORE the write lock is acquired
    /// 2. This handler calls GetById which acquires a read lock
    /// 3. Multiple threads each hold read locks and then try to upgrade to write locks
    /// 4. SQL Server detects this as a deadlock; SQLite hangs indefinitely
    /// </remarks>
    internal sealed class ReadLockAcquiringMediaSavingHandler : INotificationHandler<MediaSavingNotification>
    {
        private readonly IMediaService _mediaService;

        public ReadLockAcquiringMediaSavingHandler(IMediaService mediaService) =>
            _mediaService = mediaService;

        public void Handle(MediaSavingNotification notification)
        {
            foreach (var media in notification.SavedEntities)
            {
                // This call acquires a read lock on MediaTree.
                // Before the fix, this could cause deadlocks when combined with parallel saves.
                if (media.HasIdentity)
                {
                    _mediaService.GetById(media.Id);
                }
            }
        }
    }

    #endregion
}
