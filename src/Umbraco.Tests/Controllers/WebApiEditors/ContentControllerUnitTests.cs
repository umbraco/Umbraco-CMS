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
    public class ContentControllerUnitTests
    {
        [Test]
        public void Does_Not_Throw_Exception_When_Access_Allowed_By_Path()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = -1;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();            
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F');

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Content_Found()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = -1;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(0)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;
            
            //act/assert
            Assert.Throws<HttpResponseException>(() => ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F'));
        }

        [Test]
        public void Throws_Exception_When_No_Access_By_Path()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = 9876;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F');

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Throws_Exception_When_No_Access_By_Permission()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = -1;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A", "B", "C" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F');

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Not_Throw_Exception_When_Access_Allowed_By_Permission()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = -1;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A", "F", "C" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F');

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Does_Not_Throw_Exception_When_No_Permissions_Assigned()
        {
            //arrange
            var user = new Mock<IUser>().Object;
            user.Id = 9;
            user.StartContentId = -1;
            var content = new Mock<IContent>().Object;
            content.Path = "-1,1234,5678";
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, 'F');

            //assert
            Assert.IsTrue(result);
        }

    }

    //we REALLY need a way to nicely mock the service context, etc... so we don't have to do integration tests... coming soon.

    //NOTE: The below self hosted stuff does work so need to get some tests written. Some are not possible atm because
    // of the legacy SQL calls like checking permissions.

    //[TestFixture]
    //public class ContentControllerHostedTests : BaseRoutingTest
    //{

    //    protected override DatabaseBehavior DatabaseTestBehavior
    //    {
    //        get { return DatabaseBehavior.NoDatabasePerFixture; }
    //    }

    //    public override void TearDown()
    //    {
    //        base.TearDown();
    //        UmbracoAuthorizeAttribute.Enable = true;
    //        UmbracoApplicationAuthorizeAttribute.Enable = true;
    //    }

    //    /// <summary>
    //    /// Tests to ensure that the response filter works so that any items the user
    //    /// doesn't have access to are removed
    //    /// </summary>
    //    [Test]
    //    public async void Get_By_Ids_Response_Filtered()
    //    {
    //        UmbracoAuthorizeAttribute.Enable = false;
    //        UmbracoApplicationAuthorizeAttribute.Enable = false;

    //        var baseUrl = string.Format("http://{0}:9876", Environment.MachineName);
    //        var url = baseUrl + "/api/Content/GetByIds?ids=1&ids=2";

    //        var routingCtx = GetRoutingContext(url, 1234, null, true);

    //        var config = new HttpSelfHostConfiguration(baseUrl);
    //        using (var server = new HttpSelfHostServer(config))
    //        {
    //            var route = config.Routes.MapHttpRoute("test", "api/Content/GetByIds",
    //            new
    //            {
    //                controller = "Content",
    //                action = "GetByIds",
    //                id = RouteParameter.Optional
    //            });
    //            route.DataTokens["Namespaces"] = new string[] { "Umbraco.Web.Editors" };

    //            var client = new HttpClient(server);

    //            var request = new HttpRequestMessage
    //            {
    //                RequestUri = new Uri(url),
    //                Method = HttpMethod.Get
    //            };

    //            var result = await client.SendAsync(request);
    //        }
             
    //    }

    //}
}
