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
        public void Access_Allowed_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, null, contentService, 1234);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Content_Found()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(0)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;
            
            //act/assert
            Assert.Throws<HttpResponseException>(() => ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, new[] { 'F' }));
        }

        [Test]
        public void No_Access_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentId).Returns(9876);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new List<EntityPermission>();
            userServiceMock.Setup(x => x.GetPermissions(user, 1234)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, new[] { 'F'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void No_Access_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
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
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, new[] { 'F'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_Allowed_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
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
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, 1234, new[] { 'F'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            
            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, null, null, -1);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;
            
            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, null, null, -20);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(1234);
            var user = userMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, null, null, -20);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(1234);
            var user = userMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, null, null, -1);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Root_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, -1)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, null, -1, new[] { 'A'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Root_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, -1)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, null, -1, new[] { 'B'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, -20)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, null, -20, new[] { 'A'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentId).Returns(-1);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new List<EntityPermission>
                {
                    new EntityPermission(9, 1234, new string[]{ "A" })
                };
            userServiceMock.Setup(x => x.GetPermissions(user, -20)).Returns(permissions);
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, null, -20, new[] { 'B'});

            //assert
            Assert.IsFalse(result);
        }

    }

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
