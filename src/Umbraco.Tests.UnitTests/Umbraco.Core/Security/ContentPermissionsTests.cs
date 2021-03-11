// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security
{
    [TestFixture]
    public class ContentPermissionsTests
    {
        [Test]
        public void Access_Allowed_By_Path()
        {
            // Arrange
            IUser user = CreateUser(id: 9);
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            IContent content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            IContentService contentService = contentServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            IUserService userService = userServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent);

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Content_Found()
        {
            // Arrange
            IUser user = CreateUser(id: 9);
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            IContent content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(0)).Returns(content);
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection();
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.NotFound, result);
        }

        [Test]
        public void No_Access_By_Path()
        {
            // Arrange
            IUser user = CreateUser(id: 9, startContentId: 9876);
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            IContent content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection();
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 9876 && entity.Path == "-1,9876") });
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void No_Access_By_Permission()
        {
            // Arrange
            IUser user = CreateUser(id: 9);
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            IContent content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[] { "A", "B", "C" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_Allowed_By_Permission()
        {
            // Arrange
            IUser user = CreateUser(id: 9);
            var contentMock = new Mock<IContent>();
            contentMock.Setup(c => c.Path).Returns("-1,1234,5678");
            IContent content = contentMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock.Setup(x => x.GetById(1234)).Returns(content);
            IContentService contentService = contentServiceMock.Object;
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[] { "A", "F", "C" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1,1234,5678")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(1234, user, out IContent foundContent, new[] { 'F' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void Access_To_Root_By_Path()
        {
            // Arrange
            IUser user = CreateUser();
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-1, user, out IContent _);

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Path()
        {
            // Arrange
            IUser user = CreateUser();
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-20, user, out IContent _);

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Path()
        {
            // Arrange
            IUser user = CreateUser(startContentId: 1234);
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent);

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void No_Access_To_Root_By_Path()
        {
            // Arrange
            IUser user = CreateUser(startContentId: 1234);

            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var userServiceMock = new Mock<IUserService>();
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 1234 && entity.Path == "-1,1234") });
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent);

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_To_Root_By_Permission()
        {
            // Arrange
            IUser user = CreateUser();

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[] { "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent, new[] { 'A' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Root_By_Permission()
        {
            // Arrange
            IUser user = CreateUser(withUserGroup: false);

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[] { "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-1")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-1, user, out IContent foundContent, new[] { 'B' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        [Test]
        public void Access_To_Recycle_Bin_By_Permission()
        {
            // Arrange
            IUser user = CreateUser();

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
            {
                new EntityPermission(9876, 1234, new string[] { "A" })
            };
            var permissionSet = new EntityPermissionSet(-20, permissions);

            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent, new[] { 'A' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Granted, result);
        }

        [Test]
        public void No_Access_To_Recycle_Bin_By_Permission()
        {
            // Arrange
            IUser user = CreateUser(withUserGroup: false);

            var userServiceMock = new Mock<IUserService>();
            var permissions = new EntityPermissionCollection
                {
                    new EntityPermission(9876, 1234, new string[] { "A" })
                };
            var permissionSet = new EntityPermissionSet(1234, permissions);
            userServiceMock.Setup(x => x.GetPermissionsForPath(user, "-20")).Returns(permissionSet);
            IUserService userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            IEntityService entityService = entityServiceMock.Object;
            var contentServiceMock = new Mock<IContentService>();
            IContentService contentService = contentServiceMock.Object;
            var contentPermissions = new ContentPermissions(userService, contentService, entityService, AppCaches.Disabled);

            // Act
            ContentPermissions.ContentAccess result = contentPermissions.CheckPermissions(-20, user, out IContent foundContent, new[] { 'B' });

            // Assert
            Assert.AreEqual(ContentPermissions.ContentAccess.Denied, result);
        }

        private IUser CreateUser(int id = 0, int? startContentId = null, bool withUserGroup = true)
        {
            UserBuilder<object> builder = new UserBuilder()
                .WithId(id)
                .WithStartContentIds(startContentId.HasValue ? new[] { startContentId.Value } : new int[0]);
            if (withUserGroup)
            {
                builder = builder
                .AddUserGroup()
                    .WithId(1)
                    .WithName("admin")
                    .WithAlias("admin")
                    .Done();
            }

            return builder.Build();
        }
    }
}
