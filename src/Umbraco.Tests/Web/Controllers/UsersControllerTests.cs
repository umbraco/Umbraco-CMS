using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using Microsoft.AspNet.Identity;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Tests.Web.Controllers
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [TestFixture]
    public class UsersControllerTests : BaseDatabaseFactoryTest
    {
        protected override void FreezeResolution()
        {
            FeaturesResolver.Current = new FeaturesResolver(new UmbracoFeatures());
            base.FreezeResolution();
        }

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
                    .Returns(new[] { Mock.Of<IUserGroup>(group => group.Id == 123 && group.Alias == "writers" && group.Name == "Writers") });
                userServiceMock.Setup(service => service.GetUserGroupsByAlias(It.IsAny<string[]>()))
                    .Returns(new[] { Mock.Of<IUserGroup>(group => group.Id == 123 && group.Alias == "writers" && group.Name == "Writers") });
                userServiceMock.Setup(service => service.GetUserById(It.IsAny<int>()))
                    .Returns(new User(1234, "Test", "test@test.com", "test@test.com", "", new List<IReadOnlyUserGroup>(), new int[0], new int[0]));
                
                //we need to manually apply automapper mappings with the mocked applicationcontext
                InitializeMappers(helper.UmbracoContext.Application);

                return new UsersController(helper.UmbracoContext);
            });

            var userSave = new UserSave
            {
                Id = 1234,
                Email = "test@test.com",
                Username = "test@test.com",
                Culture = "en",
                Name = "Test",
                UserGroups = new[] { "writers" }
            };
            var response = await runner.Execute("Users", "PostSaveUser", HttpMethod.Post,
                new ObjectContent<UserSave>(userSave, new JsonMediaTypeFormatter()));

            var obj = JsonConvert.DeserializeObject<UserDisplay>(response.Item2);

            Assert.AreEqual(userSave.Name, obj.Name);
            Assert.AreEqual(1234, obj.Id);
            Assert.AreEqual(userSave.Email, obj.Email);
            var userGroupAliases = obj.UserGroups.Select(x => x.Alias).ToArray();
            foreach (var group in userSave.UserGroups)
            {
                Assert.IsTrue(userGroupAliases.Contains(group));
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
                userServiceMock.Setup(service => service.GetAll(
                    It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(),
                    It.IsAny<UserState[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<IQuery<IUser>>()))
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
