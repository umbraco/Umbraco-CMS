using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class MediaServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Move_Media()
        {
            // Arrange
            var mediaItems = CreateTrashedTestMedia();
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(mediaItems.Item3.Id);

            // Act
            mediaService.Move(media, mediaItems.Item2.Id);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item2.Id));
            Assert.That(media.Trashed, Is.False);
        }

        [Test]
        public void Can_Move_Media_To_RecycleBin()
        {
            // Arrange
            var mediaItems = CreateTrashedTestMedia();
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(mediaItems.Item1.Id);

            // Act
            mediaService.MoveToRecycleBin(media);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(-21));
            Assert.That(media.Trashed, Is.True);
        }

        [Test]
        public void Can_Move_Media_From_RecycleBin()
        {
            // Arrange
            var mediaItems = CreateTrashedTestMedia();
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(mediaItems.Item4.Id);

            // Act - moving out of recycle bin
            mediaService.Move(media, mediaItems.Item1.Id);
            var mediaChild = mediaService.GetById(mediaItems.Item5.Id);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(mediaItems.Item1.Id));
            Assert.That(media.Trashed, Is.False);
            Assert.That(mediaChild.ParentId, Is.EqualTo(mediaItems.Item4.Id));
            Assert.That(mediaChild.Trashed, Is.False);
        }

        [Test]
        public void Ensure_Content_Xml_Created()
        {
            var mediaService = ServiceContext.MediaService;
            var mediaType = MockedContentTypes.CreateVideoMediaType();
            ServiceContext.ContentTypeService.Save(mediaType);
            var media = mediaService.CreateMedia("Test", -1, "video");

            mediaService.Save(media);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var uow = provider.GetUnitOfWork();

            Assert.IsTrue(uow.Database.Exists<ContentXmlDto>(media.Id));

        }

        private Tuple<IMedia, IMedia, IMedia, IMedia, IMedia> CreateTrashedTestMedia()
        {
            //Create and Save folder-Media -> 1050
            var folderMediaType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder);
            
            //Create and Save folder-Media -> 1051
            var folder2 = MockedMedia.CreateMediaFolder(folderMediaType, -1);
            ServiceContext.MediaService.Save(folder2);
            
            //Create and Save image-Media  -> 1052
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);
            var image = (Media)MockedMedia.CreateMediaImage(imageMediaType, 1050);
            ServiceContext.MediaService.Save(image);
            
            //Create and Save folder-Media that is trashed -> 1053
            var folderTrashed = (Media)MockedMedia.CreateMediaFolder(folderMediaType, -21);
            folderTrashed.Trashed = true;
            ServiceContext.MediaService.Save(folderTrashed);
            
            //Create and Save image-Media child of folderTrashed -> 1054            
            var imageTrashed = (Media)MockedMedia.CreateMediaImage(imageMediaType, folderTrashed.Id);
            imageTrashed.Trashed = true;
            ServiceContext.MediaService.Save(imageTrashed);


            return new Tuple<IMedia, IMedia, IMedia, IMedia, IMedia>(folder, folder2, image, folderTrashed, imageTrashed);
        }
    }
}
