using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Umbraco.Tests.Services
{
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
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(1052);

            // Act
            mediaService.Move(media, 1051);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(1051));
            Assert.That(media.Trashed, Is.False);
        }

        [Test]
        public void Can_Move_Media_To_RecycleBin()
        {
            // Arrange
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(1050);

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
            var mediaService = ServiceContext.MediaService;
            var media = mediaService.GetById(1053);

            // Act - moving out of recycle bin
            mediaService.Move(media, 1050);
            var mediaChild = mediaService.GetById(1054);

            // Assert
            Assert.That(media.ParentId, Is.EqualTo(1050));
            Assert.That(media.Trashed, Is.False);
            Assert.That(mediaChild.ParentId, Is.EqualTo(1053));
            Assert.That(mediaChild.Trashed, Is.False);
        }
    }
}
