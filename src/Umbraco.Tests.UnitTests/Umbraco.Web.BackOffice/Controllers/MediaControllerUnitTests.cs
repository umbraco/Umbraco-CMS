using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Exceptions;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class MediaControllerUnitTests
    {
        [Test]
        public void Access_Allowed_By_Path()
        {
            //arrange
            var user = CreateUser(id: 9);
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, 1234);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Media_Found()
        {
            //arrange
            var user = CreateUser(id: 9);
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(0)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act/assert
            Assert.Throws<HttpResponseException>(() => MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, 1234));
        }

        [Test]
        public void No_Access_By_Path()
        {
            //arrange
            var user = CreateUser(id: 9, startMediaId: 9876);
            var mediaMock = new Mock<IMedia>();
            mediaMock.Setup(m => m.Path).Returns("-1,1234,5678");
            var media = mediaMock.Object;
            var mediaServiceMock = new Mock<IMediaService>();
            mediaServiceMock.Setup(x => x.GetById(1234)).Returns(media);
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, 1234);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            //arrange
            var user = CreateUser();
            var mediaServiceMock = new Mock<IMediaService>();
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, -1);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            //arrange
            var user = CreateUser(startMediaId: 1234);
            var mediaServiceMock = new Mock<IMediaService>();
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, -1);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var user = CreateUser();
            var mediaServiceMock = new Mock<IMediaService>();
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, -21);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var user = CreateUser(startMediaId: 1234);
            var mediaServiceMock = new Mock<IMediaService>();
            var mediaService = mediaServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;

            //act
            var result = MediaController.CheckPermissions(new Dictionary<object, object>(), user, mediaService, entityService, -21);

            //assert
            Assert.IsFalse(result);
        }

        private IUser CreateUser(int id = 0, int? startMediaId = null)
        {
            return new UserBuilder()
                .WithId(id)
                .WithStartMediaIds(startMediaId.HasValue ? new[] { startMediaId.Value } : new int[0])
                .AddUserGroup()
                    .WithId(1)
                    .WithName("admin")
                    .WithAlias("admin")
                    .Done()
                .Build();
        }
    }
}
