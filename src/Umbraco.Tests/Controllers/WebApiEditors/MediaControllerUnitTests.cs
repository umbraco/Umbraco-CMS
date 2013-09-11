using System.Collections.Generic;
using System.Web.Http;
using NUnit.Framework;
using Rhino.Mocks;
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
            var user = MockRepository.GenerateStub<IUser>();
            user.Id = 9;
            user.StartMediaId = -1;
            var media = MockRepository.GenerateStub<IMedia>();
            media.Path = "-1,1234,5678";
            var mediaService = MockRepository.GenerateStub<IMediaService>();
            mediaService.Stub(x => x.GetById(1234)).Return(media);
            
            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Media_Found()
        {
            //arrange
            var user = MockRepository.GenerateStub<IUser>();
            user.Id = 9;
            user.StartMediaId = -1;
            var media = MockRepository.GenerateStub<IMedia>();
            media.Path = "-1,1234,5678";
            var mediaService = MockRepository.GenerateStub<IMediaService>();
            mediaService.Stub(x => x.GetById(0)).Return(media);
            
            //act/assert
            Assert.Throws<HttpResponseException>(() => MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234));
        }

        [Test]
        public void Throws_Exception_When_No_Access_By_Path()
        {
            //arrange
            var user = MockRepository.GenerateStub<IUser>();
            user.Id = 9;
            user.StartMediaId = 9876;
            var media = MockRepository.GenerateStub<IMedia>();
            media.Path = "-1,1234,5678";
            var mediaService = MockRepository.GenerateStub<IMediaService>();
            mediaService.Stub(x => x.GetById(1234)).Return(media);
            
            //act
            var result = MediaController.CheckPermissions(new Dictionary<string, object>(), user, mediaService, 1234);

            //assert
            Assert.IsFalse(result);
        }

    }
}