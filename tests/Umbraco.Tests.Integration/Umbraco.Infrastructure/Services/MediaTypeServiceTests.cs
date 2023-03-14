// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
public class MediaTypeServiceTests : UmbracoIntegrationTest
{
    private MediaService MediaService => (MediaService)GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<MediaMovedToRecycleBinNotification, ContentNotificationHandler>();

    [Test]
    public void Get_With_Missing_Guid()
    {
        // Arrange
        // Act
        var result = MediaTypeService.Get(Guid.NewGuid());

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void Empty_Description_Is_Always_Null_After_Saving_Media_Type()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("mediaType", "Media Type");
        mediaType.Description = null;
        MediaTypeService.Save(mediaType);

        var mediaType2 = MediaTypeBuilder.CreateSimpleMediaType("mediaType2", "Media Type 2");
        mediaType2.Description = string.Empty;
        MediaTypeService.Save(mediaType2);

        Assert.IsNull(mediaType.Description);
        Assert.IsNull(mediaType2.Description);
    }

    [Test]
    public void Deleting_Media_Type_With_Hierarchy_Of_Media_Items_Moves_Orphaned_Media_To_Recycle_Bin()
    {
        IMediaType contentType1 = MediaTypeBuilder.CreateSimpleMediaType("test1", "Test1");
        MediaTypeService.Save(contentType1);
        IMediaType contentType2 = MediaTypeBuilder.CreateSimpleMediaType("test2", "Test2");
        MediaTypeService.Save(contentType2);
        IMediaType contentType3 = MediaTypeBuilder.CreateSimpleMediaType("test3", "Test3");
        MediaTypeService.Save(contentType3);

        IMediaType[] contentTypes = { contentType1, contentType2, contentType3 };
        var parentId = -1;

        var ids = new List<int>();

        for (var i = 0; i < 2; i++)
        {
            for (var index = 0; index < contentTypes.Length; index++)
            {
                var contentType = contentTypes[index];
                var contentItem = MediaBuilder.CreateSimpleMedia(contentType, "MyName_" + index + "_" + i, parentId);
                MediaService.Save(contentItem);
                parentId = contentItem.Id;

                ids.Add(contentItem.Id);
            }
        }

        // delete the first content type, all other content of different content types should be in the recycle bin
        MediaTypeService.Delete(contentTypes[0]);

        var found = MediaService.GetByIds(ids);

        Assert.AreEqual(4, found.Count());
        foreach (var content in found)
        {
            Assert.IsTrue(content.Trashed);
        }
    }

    [Test]
    public void Deleting_Media_Types_With_Hierarchy_Of_Media_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items()
    {
        ContentNotificationHandler.MovedMediaToRecycleBin = MovedMediaToRecycleBin;

        try
        {
            IMediaType contentType1 = MediaTypeBuilder.CreateSimpleMediaType("test1", "Test1");
            MediaTypeService.Save(contentType1);
            IMediaType contentType2 = MediaTypeBuilder.CreateSimpleMediaType("test2", "Test2");
            MediaTypeService.Save(contentType2);
            IMediaType contentType3 = MediaTypeBuilder.CreateSimpleMediaType("test3", "Test3");
            MediaTypeService.Save(contentType3);

            IMediaType[] contentTypes = { contentType1, contentType2, contentType3 };
            var parentId = -1;

            var ids = new List<int>();

            for (var i = 0; i < 2; i++)
            {
                for (var index = 0; index < contentTypes.Length; index++)
                {
                    var contentType = contentTypes[index];
                    var contentItem =
                        MediaBuilder.CreateSimpleMedia(contentType, "MyName_" + index + "_" + i, parentId);
                    MediaService.Save(contentItem);
                    parentId = contentItem.Id;

                    ids.Add(contentItem.Id);
                }
            }

            foreach (var contentType in contentTypes.Reverse())
            {
                MediaTypeService.Delete(contentType);
            }
        }
        finally
        {
            ContentNotificationHandler.MovedMediaToRecycleBin = null;
        }
    }

    private void MovedMediaToRecycleBin(MediaMovedToRecycleBinNotification notification)
    {
        foreach (var item in notification.MoveInfoCollection)
        {
            // if this item doesn't exist then Fail!
            var exists = MediaService.GetById(item.Entity.Id);
            if (exists == null)
            {
                Assert.Fail("The item doesn't exist");
            }
        }
    }

    [Test]
    public void Can_Copy_MediaType_By_Performing_Clone()
    {
        // Arrange
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2") as IMediaType;
        MediaTypeService.Save(mediaType);

        // Act
        var sut = mediaType.DeepCloneWithResetIdentities("Image2_2");
        Assert.IsNotNull(sut);
        MediaTypeService.Save(sut);

        // Assert
        Assert.That(sut.HasIdentity, Is.True);
        Assert.AreEqual(mediaType.ParentId, sut.ParentId);
        Assert.AreEqual(mediaType.Level, sut.Level);
        Assert.AreEqual(mediaType.PropertyTypes.Count(), sut.PropertyTypes.Count());
        Assert.AreNotEqual(mediaType.Id, sut.Id);
        Assert.AreNotEqual(mediaType.Key, sut.Key);
        Assert.AreNotEqual(mediaType.Path, sut.Path);
        Assert.AreNotEqual(mediaType.SortOrder, sut.SortOrder);
        Assert.AreNotEqual(mediaType.PropertyTypes.First(x => x.Alias.Equals("umbracoFile")).Id,
            sut.PropertyTypes.First(x => x.Alias.Equals("umbracoFile")).Id);
        Assert.AreNotEqual(mediaType.PropertyGroups.First(x => x.Name.Equals("Media")).Id,
            sut.PropertyGroups.First(x => x.Name.Equals("Media")).Id);
    }

    [Test]
    public void Can_Copy_MediaType_To_New_Parent_By_Performing_Clone()
    {
        // Arrange
        var parentMediaType1 = MediaTypeBuilder.CreateSimpleMediaType("parent1", "Parent1");
        MediaTypeService.Save(parentMediaType1);
        var parentMediaType2 = MediaTypeBuilder.CreateSimpleMediaType("parent2", "Parent2", null, true);
        MediaTypeService.Save(parentMediaType2);
        var mediaType = MediaTypeBuilder.CreateImageMediaType("Image2") as IMediaType;
        MediaTypeService.Save(mediaType);

        // Act
        var clone = mediaType.DeepCloneWithResetIdentities("newcategory");
        Assert.IsNotNull(clone);
        clone.RemoveContentType("parent1");
        clone.AddContentType(parentMediaType2);
        clone.ParentId = parentMediaType2.Id;
        MediaTypeService.Save(clone);

        // Assert
        Assert.That(clone.HasIdentity, Is.True);

        var clonedMediaType = MediaTypeService.Get(clone.Id);
        var originalMediaType = MediaTypeService.Get(mediaType.Id);

        Assert.That(clonedMediaType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
        Assert.That(clonedMediaType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

        Assert.AreEqual(clonedMediaType.Path, "-1," + parentMediaType2.Id + "," + clonedMediaType.Id);
        Assert.AreEqual(clonedMediaType.PropertyTypes.Count(), originalMediaType.PropertyTypes.Count());

        Assert.AreNotEqual(clonedMediaType.ParentId, originalMediaType.ParentId);
        Assert.AreEqual(clonedMediaType.ParentId, parentMediaType2.Id);

        Assert.AreNotEqual(clonedMediaType.Id, originalMediaType.Id);
        Assert.AreNotEqual(clonedMediaType.Key, originalMediaType.Key);
        Assert.AreNotEqual(clonedMediaType.Path, originalMediaType.Path);

        Assert.AreNotEqual(clonedMediaType.PropertyTypes.First(x => x.Alias.StartsWith("umbracoFile")).Id,
            originalMediaType.PropertyTypes.First(x => x.Alias.StartsWith("umbracoFile")).Id);
        Assert.AreNotEqual(clonedMediaType.PropertyGroups.First(x => x.Name.StartsWith("Media")).Id,
            originalMediaType.PropertyGroups.First(x => x.Name.StartsWith("Media")).Id);
    }

    public class ContentNotificationHandler :
        INotificationHandler<MediaMovedToRecycleBinNotification>
    {
        public static Action<MediaMovedToRecycleBinNotification> MovedMediaToRecycleBin { get; set; }

        public void Handle(MediaMovedToRecycleBinNotification notification) =>
            MovedMediaToRecycleBin?.Invoke(notification);
    }
}
