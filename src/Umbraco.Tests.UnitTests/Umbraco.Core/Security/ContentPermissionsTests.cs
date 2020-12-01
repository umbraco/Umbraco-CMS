using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Security
{
    [TestFixture]
    public class ContentPermissionsTests
    {
        [Test]
        public void Access_Allowed_By_Path()
        {
            //arrange
            var user = CreateUser(id: 9);
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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent);

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Content_Found()
        {
            //arrange
            var user = CreateUser(id: 9);
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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.NotFound, result);
        }

        [Test]
        public void No_Access_By_Path()
        {
            //arrange
            var user = CreateUser(id: 9, startContentId: 9876);
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
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
            var entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void No_Access_By_Permission()
        {
            //arrange
            var user = CreateUser(id: 9);
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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_Allowed_By_Permission()
        {
            //arrange
            var user = CreateUser(id: 9);
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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            //arrange
            var user = CreateUser();
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-1, user, out IContent _);

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var user = CreateUser();
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            var entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-20, user, out IContent _);

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            //arrange
            var user = CreateUser(startContentId: 1234);
            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent);

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            //arrange
            var user = CreateUser(startContentId: 1234);

            var contentServiceMock = new Mock<IContentService>();
            var contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            var entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent);

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_To_Root_By_Permission()
        {
            //arrange
            var user = CreateUser();

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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent, new[] { 'A' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Root_By_Permission()
        {
            //arrange
            var user = CreateUser(withUserGroup: false);

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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent, new[] { 'B' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var user = CreateUser();

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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent, new[] { 'A' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Permission()
        {
            //arrange
            var user = CreateUser(withUserGroup: false);

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
            var contentPermissions = new ContentPermissions(userService, contentService, entityService);

            //act
            var result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent, new[] { 'B' });

            //assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        private IUser CreateUser(int id = 0, int? startContentId = null, bool withUserGroup = true)
        {
            var builder = new UserBuilder()
                .WithId(id)
                .WithStartContentIds(startContentId.HasValue ? new[] { startContentId.Value } : new int[0]);
            if (withUserGroup)
                builder = builder
                .AddUserGroup()
                    .WithId(1)
                    .WithName("admin")
                    .WithAlias("admin")
                    .Done();

            return builder.Build();
        }
    }

}
