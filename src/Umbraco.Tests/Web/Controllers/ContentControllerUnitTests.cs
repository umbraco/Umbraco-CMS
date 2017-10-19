﻿using System.Collections.Generic;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Tests.Web.Controllers
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
            userMock.Setup(u => u.Groups).Returns(new[] { new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>()) });
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, 1234);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Throws_Exception_When_No_Content_Found()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(0)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new EntityPermissionCollection();
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act/assert
            Assert.Throws<HttpResponseException>(() => ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, 1234, new[] { 'F' }));
        }

        [Test]
        public void No_Access_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.StartContentIds).Returns(new[]{ 9876 });
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new EntityPermissionCollection();
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<EntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, 1234, new[] { 'F'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void No_Access_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();    
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[]{ "A", "B", "C" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, 1234, new[] { 'F'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_Allowed_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(9);
            userMock.Setup(u => u.Groups).Returns(new[] { new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>()) });
            var user = userMock.Object;
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            var content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            var contentService = contentServiceMock.Object;
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[]{ "A", "F", "C" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, 1234, new[] { 'F'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.Groups).Returns(new[] {new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>())});
            var user = userMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -1);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.Groups).Returns(new[] { new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>()) });
            var user = userMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -20);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentIds).Returns(new []{ 1234 });
            var user = userMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<EntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -20);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.StartContentIds).Returns(new []{ 1234 });
            var user = userMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<EntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -1);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Root_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.Groups).Returns(new[] { new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>()) });
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[]{ "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -1, new[] { 'A'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Root_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[]{ "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;            

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -1, new[] { 'B'});

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            userMock.Setup(u => u.Groups).Returns(new[] { new ReadOnlyUserGroup(1, "admin", "", -1, -1, "admin", new string[0], new List<string>()) });
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
            {
                new EntityPermission(9876, 1234, new string[]{ "A" })
            };
            var permissionSet = new EntityPermissionSet(-20, permissions);
            
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;            

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -20, new[] { 'A'});

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.Id).Returns(0);
            var user = userMock.Object;

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[]{ "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;            

            //act
            var result = ContentController.CheckPermissions(new Dictionary<string, object>(), user, userService, contentService, entityService, -20, new[] { 'B'});

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
