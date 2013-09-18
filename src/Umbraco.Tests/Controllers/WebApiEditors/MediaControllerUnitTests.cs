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
        public void Does_Not_Throw_Exception_When_Access_Allowed_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            var user = userMock.Object;
            user.Id = 9;
            user.StartMediaId = -1;
            var mediaMock = new Mock<IMedia>();
            var media = mediaMock.Object;
            media.Path = "-1,1234,5678";
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
            var user = userMock.Object;
            user.Id = 9;
            user.StartMediaId = -1;
            var mediaMock = new Mock<IMedia>();
            var media = mediaMock.Object;
            media.Path = "-1,1234,5678";
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(0)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            
            //act/assert
            Assert.Throws<HttpResponseException>(() => MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234));
        }

        [Test]
        public void Throws_Exception_When_No_Access_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            var user = userMock.Object;
            user.Id = 9;
            user.StartMediaId = 9876;
            var mediaMock = new Mock<IMedia>();
            var media = mediaMock.Object;
            media.Path = "-1,1234,5678";
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(0)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            
            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234);

            //assert
            Assert.IsFalse(result);
        }

    }
}