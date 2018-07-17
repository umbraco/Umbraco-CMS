﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class UsersControllerTests : TestWithDatabaseBase
    {
        protected override void ComposeApplication(bool withApplication)
        {
            base.ComposeApplication(withApplication);
            //if (!withApplication) return;

            // replace the true IUserService implementation with a mock
            // so that each test can configure the service to their liking
            Container.RegisterSingleton(f => Mock.Of<IUserService>());

            // kill the true IEntityService too
            Container.RegisterSingleton(f => Mock.Of<IEntityService>());
            
            Container.RegisterSingleton<UmbracoFeatures>();
        }

        [Test]
        public async System.Threading.Tasks.Task Save_User()
        {
            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                //setup some mocks
                Umbraco.Core.Configuration.GlobalSettings.HasSmtpServer = true;

                var userServiceMock = Mock.Get(Current.Services.UserService);

                userServiceMock.Setup(service => service.Save(It.IsAny<IUser>()))
                    .Callback((IUser u) =>
                    {
                        u.Id = 1234;
                    });
                userServiceMock.Setup(service => service.GetAllUserGroups(It.IsAny<int[]>()))
                    .Returns(new[] { Mock.Of<IUserGroup>(group => group.Id == 123 && group.Alias == "writers" && group.Name == "Writers") });
                userServiceMock.Setup(service => service.GetUserGroupsByAlias(It.IsAny<string[]>()))
                    .Returns(new[] { Mock.Of<IUserGroup>(group => group.Id == 123 && group.Alias == "writers" && group.Name == "Writers") });
                userServiceMock.Setup(service => service.GetUserById(It.IsAny<int>()))
                    .Returns((int id) => id == 1234 ? new User(1234, "Test", "test@test.com", "test@test.com", "", new List<IReadOnlyUserGroup>(), new int[0], new int[0]) : null);

                var usersController = new UsersController();
                Container.InjectProperties(usersController);
                return usersController;
            }

            var userSave = new UserSave
            {
                Id = 1234,
                Email = "test@test.com",
                Username = "test@test.com",
                Culture = "en",
                Name = "Test",
                UserGroups = new[] { "writers" }
            };

            var runner = new TestRunner(Factory);
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

        private void MockForGetPagedUsers()
        {
            Mock.Get(Current.SqlContext)
                .Setup(x => x.Query<IUser>())
                .Returns(new Query<IUser>(Current.SqlContext));

            var syntax = new SqlCeSyntaxProvider();

            Mock.Get(Current.SqlContext)
                .Setup(x => x.SqlSyntax)
                .Returns(syntax);

            var mappers = new MapperCollection(new []
            {
                new UserMapper()
            });

            Mock.Get(Current.SqlContext)
                .Setup(x => x.Mappers)
                .Returns(mappers);
        }

        [Test]
        public async System.Threading.Tasks.Task GetPagedUsers_Empty()
        {
            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                var usersController = new UsersController();
                Container.InjectProperties(usersController);
                return usersController;
            }

            MockForGetPagedUsers();

            var runner = new TestRunner(Factory);
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async System.Threading.Tasks.Task GetPagedUsers_10()
        {
            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                //setup some mocks
                var userServiceMock = Mock.Get(Current.Services.UserService);
                var users = MockedUser.CreateMulipleUsers(10);
                long outVal = 10;
                userServiceMock.Setup(service => service.GetAll(
                        It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(),
                        It.IsAny<UserState[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<IQuery<IUser>>()))
                    .Returns(() => users);

                var usersController = new UsersController();
                Container.InjectProperties(usersController);
                return usersController;
            }

            MockForGetPagedUsers();

            var runner = new TestRunner(Factory);
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(10, obj.TotalItems);
            Assert.AreEqual(10, obj.Items.Count());
        }
    }
}
