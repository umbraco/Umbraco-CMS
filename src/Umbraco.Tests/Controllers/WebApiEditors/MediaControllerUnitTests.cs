using System.Collections.Generic;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Tests.Controllers.WebApiEditors
{
    [TestFixture]
    public class MediaControllerUnitTests
    {
        [Test]
        public void Access_Allowed_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartMediaId).Returns(-1);
            var user = userMock.Object;
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();            
            mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
            var mediaService = mediaServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Media_Found()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartMediaId).Returns(-1);
            var user = userMock.Object;
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(0)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            
            //act/assert
            Assert.Throws<HttpResponseException>(() => MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234));
        }

        [Test]
        public void No_Access_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartMediaId).Returns(9876);
            var user = userMock.Object;
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            
            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartMediaId).Returns(-1);
            var user = userMock.Object;
            
            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, null, -1);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartMediaId).Returns(1234);
            var user = userMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, null, -1);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartMediaId).Returns(-1);
            var user = userMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, null, -21);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartMediaId).Returns(1234);
            var user = userMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, null, -21);

            //assert
            Assert.IsFalse(result);
        }
    }
}