using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class UserEditorAuthorizationHelperTests
    {
        [Test]
        public void Admin_Is_Authorized()
        {
            var currentUser = GetAdminUser();
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new string[0]);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Non_Admin_Cannot_Save_Admin()
        {
            var currentUser = Mock.Of<IUser>();
            var savingUser = GetAdminUser();

            var contentService = new Mock<IContentService>();
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new string[0]);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Cannot_Grant_Group_Membership_Without_Being_A_Member()
        {
            var currentUser = Mock.Of<IUser>(user => user.Groups == new[]
            {
                new ReadOnlyUserGroup(1, "Test", "icon-user", null, null, "test", new string[0], new string[0])
            });
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new[] {"FunGroup"});

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Can_Grant_Group_Membership_With_Being_A_Member()
        {
            var currentUser = Mock.Of<IUser>(user => user.Groups == new[]
            {
                new ReadOnlyUserGroup(1, "Test", "icon-user", null, null, "test", new string[0], new string[0])
            });
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new int[0], new[] { "test" });

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Cannot_Grant_Content_Start_Node_On_User_Without_Access()
        {
            var currentUser = Mock.Of<IUser>(user => user.StartContentIds == new[]{9876});
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();
            contentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(Mock.Of<IContent>(content => content.Path == "-1,1234"));
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();
            entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { new EntityPath() { Path = "-1,9876", Id = 9876 } });

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new []{1234}, new int[0], new string[0]);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Can_Grant_Content_Start_Node_On_User_With_Access()
        {
            var currentUser = Mock.Of<IUser>(user => user.StartContentIds == new[] { 9876 });
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();
            contentService.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Mock.Of<IContent>(content => content.Path == "-1,9876,5555"));
            var mediaService = new Mock<IMediaService>();
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();
            entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { new EntityPath() { Path = "-1,9876", Id = 9876 } });

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new[] { 5555 }, new int[0], new string[0]);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Cannot_Grant_Media_Start_Node_On_User_Without_Access()
        {
            var currentUser = Mock.Of<IUser>(user => user.StartMediaIds == new[] { 9876 });
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();            
            var mediaService = new Mock<IMediaService>();
            mediaService.Setup(x => x.GetById(It.IsAny<int>())).Returns(Mock.Of<IMedia>(content => content.Path == "-1,1234"));
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();
            entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { new EntityPath() { Path = "-1,9876", Id = 9876 } });

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] {1234}, new string[0]);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Can_Grant_Media_Start_Node_On_User_With_Access()
        {
            var currentUser = Mock.Of<IUser>(user => user.StartMediaIds == new[] { 9876 });
            var savingUser = Mock.Of<IUser>();

            var contentService = new Mock<IContentService>();            
            var mediaService = new Mock<IMediaService>();
            mediaService.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns(Mock.Of<IMedia>(content => content.Path == "-1,9876,5555"));
            var userService = new Mock<IUserService>();
            var entityService = new Mock<IEntityService>();
            entityService.Setup(service => service.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { new EntityPath() { Path = "-1,9876", Id = 9876 } });

            var authHelper = new UserEditorAuthorizationHelper(
                contentService.Object,
                mediaService.Object,
                userService.Object,
                entityService.Object);

            var result = authHelper.IsAuthorized(currentUser, savingUser, new int[0], new[] { 5555 }, new string[0]);

            Assert.IsTrue(result.Success);
        }

        private IUser GetAdminUser()
        {
            var admin = Mock.Of<IUser>(user => user.Groups == new[]
            {
                new ReadOnlyUserGroup(1, "Admin", "icon-user", null, null, Constants.Security.AdminGroupAlias, new string[0], new string[0])
            });
            return admin;
        }
    }
}
