using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Tests.Controllers.WebApiEditors
{
    [TestFixture]
    public class FilterAllowedOutgoingContentAttributeTests
    {
        [Test]
        public void GetValueFromResponse_Already_EnumerableContent()
        {
            var att = new FilterAllowedOutgoingContentAttribute(typeof(IEnumerable<ContentItemBasic>));
            var val = new List<ContentItemBasic>() {new ContentItemBasic()};
            var result = att.GetValueFromResponse(
                new ObjectContent(typeof (IEnumerable<ContentItemBasic>),
                                  val, 
                                  new JsonMediaTypeFormatter(), 
                                  new MediaTypeHeaderValue("html/text")));

            Assert.AreEqual(val, result);
            Assert.AreEqual(1, ((IEnumerable<ContentItemBasic>)result).Count());
        }

        [Test]
        public void GetValueFromResponse_From_Property()
        {
            var att = new FilterAllowedOutgoingContentAttribute(typeof(IEnumerable<ContentItemBasic>), "MyList");
            var val = new List<ContentItemBasic>() { new ContentItemBasic() };
            var container = new MyTestClass() {MyList = val};

            var result = att.GetValueFromResponse(
                new ObjectContent(typeof(MyTestClass),
                                  container,
                                  new JsonMediaTypeFormatter(),
                                  new MediaTypeHeaderValue("html/text")));

            Assert.AreEqual(val, result);
            Assert.AreEqual(1, ((IEnumerable<ContentItemBasic>)result).Count());
        }

        [Test]
        public void GetValueFromResponse_Returns_Null_Not_Found_Property()
        {
            var att = new FilterAllowedOutgoingContentAttribute(typeof(IEnumerable<ContentItemBasic>), "DontFind");
            var val = new List<ContentItemBasic>() { new ContentItemBasic() };
            var container = new MyTestClass() { MyList = val };

            var result = att.GetValueFromResponse(
                new ObjectContent(typeof(MyTestClass),
                                  container,
                                  new JsonMediaTypeFormatter(),
                                  new MediaTypeHeaderValue("html/text")));

            Assert.AreEqual(null, result);

        }

        [Test]
        public void Filter_On_Start_Node()
        {
            var att = new FilterAllowedOutgoingContentAttribute(typeof(IEnumerable<ContentItemBasic>));
            var list = new List<dynamic>();
            var path = "";
            for (var i = 0; i < 10; i++)
            {
                if (i > 0 && path.EndsWith(",") == false)
                {
                    path += ",";
                }
                path += i.ToInvariantString();
                list.Add(new ContentItemBasic { Id = i, Name = "Test" + i, ParentId = i, Path = path });
            }            
            var user = MockRepository.GenerateStub<IUser>();
            user.Id = 9;
            user.StartContentId = 5;
            
            att.FilterBasedOnStartNode(list, user);

            Assert.AreEqual(5, list.Count);
            
        }

        [Test]
        public void Filter_On_Permissions()
        {
            var att = new FilterAllowedOutgoingContentAttribute(typeof(IEnumerable<ContentItemBasic>));
            var list = new List<dynamic>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(new ContentItemBasic{Id = i, Name = "Test" + i, ParentId = -1});
            }
            var ids = list.Select(x => (int)x.Id).ToArray();
            var user = MockRepository.GenerateStub<IUser>();
            user.Id = 9;
            user.StartContentId = -1;
            var userService = MockRepository.GenerateStub<IUserService>();
            //we're only assigning 3 nodes browse permissions so that is what we expect as a result
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1, new string[]{ "F" }),
                    new EntityPermission(9, 2, new string[]{ "F" }),
                    new EntityPermission(9, 3, new string[]{ "F" }),
                    new EntityPermission(9, 4, new string[]{ "A" })
                };
            userService.Stub(x => x.GetPermissions(user, ids)).Return(permissions);

            att.FilterBasedOnPermissions(list, user, userService);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list.ElementAt(0).Id);
            Assert.AreEqual(2, list.ElementAt(1).Id);
            Assert.AreEqual(3, list.ElementAt(2).Id);
        }

        private class MyTestClass
        {
            public IEnumerable<ContentItemBasic> MyList { get; set; } 
        }
    }

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