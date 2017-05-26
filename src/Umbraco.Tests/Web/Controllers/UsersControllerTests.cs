using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Web.Controllers
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [TestFixture]
    public class UsersControllerTests : BaseDatabaseFactoryTest
    {

        [Test]
        public async void Save_User()
        {
            var runner = new TestRunner((message, helper) =>
            {
                //setup some mocks
                Umbraco.Core.Configuration.GlobalSettings.HasSmtpServer = true;

                var userServiceMock = Mock.Get(helper.UmbracoContext.Application.Services.UserService);

                userServiceMock.Setup(service => service.Save(It.IsAny<IUser>(), It.IsAny<bool>()))
                    .Callback((IUser u, bool raiseEvents) =>
                    {
                        u.Id = 1234;
                    });
                userServiceMock.Setup(service => service.GetAllUserGroups(It.IsAny<int[]>()))
                    .Returns(Enumerable.Empty<IUserGroup>);

                //we need to manually apply automapper mappings with the mocked applicationcontext
                InitializeMappers(helper.UmbracoContext.Application);

                return new UsersController(helper.UmbracoContext);
            });

            var invite = new UserInvite
            {
                Id = -1,
                Email = "test@test.com",
                Message = "Hello test!",
                Name = "Test",
                UserGroups = new[] { "writers" }
            };
            var response = await runner.Execute("Users", "PostSaveUser", HttpMethod.Post,
                new ObjectContent<UserInvite>(invite, new JsonMediaTypeFormatter()));

            var obj = JsonConvert.DeserializeObject<UserDisplay>(response.Item2);

            Assert.AreEqual(invite.Name, obj.Name);
            Assert.AreEqual(1234, obj.Id);
            Assert.AreEqual(invite.Email, obj.Email);
            foreach (var group in invite.UserGroups)
            {
                Assert.IsTrue(obj.UserGroups.Contains(group));
            }
        }

        [Test]
        public async void Invite_User()
        {
            var runner = new TestRunner((message, helper) =>
            {
                //setup some mocks
                Umbraco.Core.Configuration.GlobalSettings.HasSmtpServer = true;

                var userServiceMock = Mock.Get(helper.UmbracoContext.Application.Services.UserService);

                userServiceMock.Setup(service => service.Save(It.IsAny<IUser>(), It.IsAny<bool>()))
                    .Callback((IUser u, bool raiseEvents) =>
                    {
                        u.Id = 1234;
                    });
                userServiceMock.Setup(service => service.GetAllUserGroups(It.IsAny<int[]>()))
                    .Returns(Enumerable.Empty<IUserGroup>);

                //we need to manually apply automapper mappings with the mocked applicationcontext
                InitializeMappers(helper.UmbracoContext.Application);

                return new UsersController(helper.UmbracoContext);
            });

            var invite = new UserInvite
            {
                Id = -1,
                Email = "test@test.com",
                Message = "Hello test!",
                Name = "Test",
                UserGroups = new[] {"writers"}
            };
            var response = await runner.Execute("Users", "PostInviteUser", HttpMethod.Post,
                new ObjectContent<UserInvite>(invite, new JsonMediaTypeFormatter()));

            var obj = JsonConvert.DeserializeObject<UserDisplay>(response.Item2);

            Assert.AreEqual(invite.Name, obj.Name);
            Assert.AreEqual(1234, obj.Id);
            Assert.AreEqual(invite.Email, obj.Email);
            foreach (var group in invite.UserGroups)
            {
                Assert.IsTrue(obj.UserGroups.Contains(group));
            }
        }

        [Test]
        public async void GetPagedUsers_Empty()
        {
            var runner = new TestRunner((message, helper) =>
            {
                //we need to manually apply automapper mappings with the mocked applicationcontext
                InitializeMappers(helper.UmbracoContext.Application);

                return new UsersController(helper.UmbracoContext);
            });
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async void GetPagedUsers_10()
        {
            var runner = new TestRunner((message, helper) =>
            {
                //setup some mocks
                var userServiceMock = Mock.Get(helper.UmbracoContext.Application.Services.UserService);
                var users = MockedUser.CreateMulipleUsers(10);
                long outVal = 10;
                userServiceMock.Setup(service => service.GetAll(It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<UserState?>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(() => users);

                //we need to manually apply automapper mappings with the mocked applicationcontext
                InitializeMappers(helper.UmbracoContext.Application);

                return new UsersController(helper.UmbracoContext);
            });
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(10, obj.TotalItems);
            Assert.AreEqual(10, obj.Items.Count());
        }
    }
}
