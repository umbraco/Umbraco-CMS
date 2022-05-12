// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class MediaServiceTests : UmbracoIntegrationTest
    {
        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

        [Test]
        public void Can_Update_Media_Property_Values()
        {
            IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test");
            MediaTypeService.Save(mediaType);
            IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "hello", -1);
            media.SetValue("title", "title of mine");
            media.SetValue("bodyText", "hello world");
            MediaService.Save(media);

            // re-get
            media = MediaService.GetById(media.Id);
            media.SetValue("title", "another title of mine");          // Change a value
            media.SetValue("bodyText", null);                          // Clear a value
            media.SetValue("author", "new author");                    // Add a value
            MediaService.Save(media);

            // re-get
            media = MediaService.GetById(media.Id);
            Assert.AreEqual("another title of mine", media.GetValue("title"));
            Assert.IsNull(media.GetValue("bodyText"));
            Assert.AreEqual("new author", media.GetValue("author"));
        }

        /// <summary>
        /// Used to list out all ambiguous events that will require dispatching with a name
        /// </summary>
        [Test]
        [Explicit]
        public void List_Ambiguous_Events()
        {
            EventInfo[] events = MediaService.GetType().GetEvents(BindingFlags.Static | BindingFlags.Public);
            Type typedEventHandler = typeof(TypedEventHandler<,>);
            foreach (EventInfo e in events)
            {
                // only continue if this is a TypedEventHandler
                if (!e.EventHandlerType.IsGenericType)
                {
                    continue;
                }

                Type typeDef = e.EventHandlerType.GetGenericTypeDefinition();
                if (typedEventHandler != typeDef)
                {
                    continue;
                }

                // get the event arg type
                Type eventArgType = e.EventHandlerType.GenericTypeArguments[1];

                Attempt<EventNameExtractorResult> found = EventNameExtractor.FindEvent(typeof(MediaService), eventArgType, EventNameExtractor.MatchIngNames);
                if (!found.Success && found.Result.Error == EventNameExtractorError.Ambiguous)
                {
                    Console.WriteLine($"Ambiguous event, source: {typeof(MediaService)}, args: {eventArgType}");
                }
            }
        }

        [Test]
        public void Get_Paged_Children_With_Media_Type_Filter()
        {
            MediaType mediaType1 = MediaTypeBuilder.CreateImageMediaType("Image2");
            MediaTypeService.Save(mediaType1);
            MediaType mediaType2 = MediaTypeBuilder.CreateImageMediaType("Image3");
            MediaTypeService.Save(mediaType2);

            for (int i = 0; i < 10; i++)
            {
                Media m1 = MediaBuilder.CreateMediaImage(mediaType1, -1);
                MediaService.Save(m1);
                Media m2 = MediaBuilder.CreateMediaImage(mediaType2, -1);
                MediaService.Save(m2);
            }

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope(autoComplete: true))
            {
                IEnumerable<IMedia> result = MediaService.GetPagedChildren(
                    -1,
                    0,
                    11,
                    out long total,
                    provider.CreateQuery<IMedia>()
                        .Where(x => new[] { mediaType1.Id, mediaType2.Id }.Contains(x.ContentTypeId)),
                    Ordering.By("SortOrder", Direction.Ascending));
                Assert.AreEqual(11, result.Count());
                Assert.AreEqual(20, total);

                result = MediaService.GetPagedChildren(
                    -1,
                    1,
                    11,
                    out total,
                    provider.CreateQuery<IMedia>()
                        .Where(x => new[] { mediaType1.Id, mediaType2.Id }.Contains(x.ContentTypeId)),
                    Ordering.By("SortOrder", Direction.Ascending));
                Assert.AreEqual(9, result.Count());
                Assert.AreEqual(20, total);
            }
        }

        [Test]
        public void Can_Move_Media()
        {
            // Arrange
            Tuple<IMedia, IMedia, IMedia, IMedia, IMedia> mediaItems = CreateTrashedTestMedia();
            IMedia media = MediaService.GetById(mediaItems.Item3.Id);

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
            Tuple<IMedia, IMedia, IMedia, IMedia, IMedia> mediaItems = CreateTrashedTestMedia();
            IMedia media = MediaService.GetById(mediaItems.Item1.Id);

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
            Tuple<IMedia, IMedia, IMedia, IMedia, IMedia> mediaItems = CreateTrashedTestMedia();
            IMedia media = MediaService.GetById(mediaItems.Item4.Id);

            // Act - moving out of recycle bin
            MediaService.Move(media, mediaItems.Item1.Id);
            IMedia mediaChild = MediaService.GetById(mediaItems.Item5.Id);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item1.Id));
            Assert.That(media.Trashed, Is.False);
            Assert.That(mediaChild.ParentId, Is.EqualTo(mediaItems.Item4.Id));
            Assert.That(mediaChild.Trashed, Is.False);
        }

        [Test]
        public void Cannot_Save_Media_With_Empty_Name()
        {
            // Arrange
            MediaType mediaType = MediaTypeBuilder.CreateNewMediaType();
            MediaTypeService.Save(mediaType);
            IMedia media = MediaService.CreateMedia(string.Empty, -1, Constants.Conventions.MediaTypes.VideoAlias);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => MediaService.Save(media));
        }


        // [Test]
        // public void Ensure_Content_Xml_Created()
        // {
        //     var mediaType = MediaTypeBuilder.CreateVideoMediaType();
        //     MediaTypeService.Save(mediaType);
        //     var media = MediaService.CreateMedia("Test", -1, Constants.Conventions.MediaTypes.VideoAlias);
        //
        //     MediaService.Save(media);
        //
        //     using (var scope = ScopeProvider.CreateScope())
        //     {
        //         Assert.IsTrue(scope.Database.Exists<ContentXmlDto>(media.Id));
        //     }
        // }

        [Test]
        public void Can_Get_Media_By_Path()
        {
            MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
            MediaTypeService.Save(mediaType);

            Media media = MediaBuilder.CreateMediaImage(mediaType, -1);
            MediaService.Save(media);

            string mediaPath = "/media/test-image.png";
            IMedia resolvedMedia = MediaService.GetMediaByPath(mediaPath);

            Assert.IsNotNull(resolvedMedia);
            Assert.That(resolvedMedia.GetValue(Constants.Conventions.Media.File).ToString() == mediaPath);
        }

        [Test]
        public void Can_Get_Media_With_Crop_By_Path()
        {
            MediaType mediaType = MediaTypeBuilder.CreateImageMediaTypeWithCrop("Image2");
            MediaTypeService.Save(mediaType);

            Media media = MediaBuilder.CreateMediaImageWithCrop(mediaType, -1);
            MediaService.Save(media);

            string mediaPath = "/media/test-image.png";
            IMedia resolvedMedia = MediaService.GetMediaByPath(mediaPath);

            Assert.IsNotNull(resolvedMedia);
            Assert.That(resolvedMedia.GetValue(Constants.Conventions.Media.File).ToString().Contains(mediaPath));
        }

        [Test]
        public void Can_Get_Paged_Children()
        {
            MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
            MediaTypeService.Save(mediaType);
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaService.Save(c1);
            }

            IMediaService service = MediaService;

            IMedia[] entities = service.GetPagedChildren(-1, 0, 6, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(-1, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void Can_Get_Paged_Children_Dont_Get_Descendants()
        {
            MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("Image2");
            MediaTypeService.Save(mediaType);

            // Only add 9 as we also add a folder with children.
            for (int i = 0; i < 9; i++)
            {
                Media m1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaService.Save(m1);
            }

            MediaType mediaTypeForFolder = MediaTypeBuilder.CreateImageMediaType("Folder2");
            MediaTypeService.Save(mediaTypeForFolder);
            Media mediaFolder = MediaBuilder.CreateMediaFolder(mediaTypeForFolder, -1);
            MediaService.Save(mediaFolder);
            for (int i = 0; i < 10; i++)
            {
                Media m1 = MediaBuilder.CreateMediaImage(mediaType, mediaFolder.Id);
                MediaService.Save(m1);
            }

            IMediaService service = MediaService;

            // Children in root including the folder - not the descendants in the folder.
            IMedia[] entities = service.GetPagedChildren(-1, 0, 6, out long total).ToArray();
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
            IMediaType folderMediaType = MediaTypeService.Get(1031);
            Media folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
            MediaService.Save(folder);

            // Create and Save folder-Media -> 1051
            Media folder2 = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
            MediaService.Save(folder2);

            // Create and Save image-Media  -> 1052
            IMediaType imageMediaType = MediaTypeService.Get(1032);
            Media image = MediaBuilder.CreateMediaImage(imageMediaType, 1050);
            MediaService.Save(image);

            // Create and Save folder-Media that is trashed -> 1053
            Media folderTrashed = MediaBuilder.CreateMediaFolder(folderMediaType, -21);
            folderTrashed.Trashed = true;
            MediaService.Save(folderTrashed);

            // Create and Save image-Media child of folderTrashed -> 1054
            Media imageTrashed = MediaBuilder.CreateMediaImage(imageMediaType, folderTrashed.Id);
            imageTrashed.Trashed = true;
            MediaService.Save(imageTrashed);

            return new Tuple<IMedia, IMedia, IMedia, IMedia, IMedia>(folder, folder2, image, folderTrashed, imageTrashed);
        }
    }
}
