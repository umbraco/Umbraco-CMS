using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
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
using Umbraco.Core.Mapping;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Web.Routing;
using Umbraco.Core.Media;
using Umbraco.Net;
using Umbraco.Persistance.SqlCe;
using Umbraco.Web.Models.Identity;
using Umbraco.Web.Security;

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
            Composition.RegisterUnique(f => Mock.Of<IUserService>());

            // kill the true IEntityService too
            Composition.RegisterUnique(f => Mock.Of<IEntityService>());

            Composition.RegisterUnique<UmbracoFeatures>();
        }

        [Test]
        public async Task Save_User()
        {
            ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor)
            {
                var userServiceMock = Mock.Get(ServiceContext.UserService);

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
                    .Returns((int id) => id == 1234 ? new User(TestObjects.GetGlobalSettings(), 1234, "Test", "test@test.com", "test@test.com", "", new List<IReadOnlyUserGroup>(), new int[0], new int[0]) : null);

                var usersController = new UsersController(
                    Factory.GetInstance<IGlobalSettings>(),
                    umbracoContextAccessor,
                    Factory.GetInstance<ISqlContext>(),
                    Factory.GetInstance<ServiceContext>(),
                    Factory.GetInstance<AppCaches>(),
                    Factory.GetInstance<IProfilingLogger>(),
                    Factory.GetInstance<IRuntimeState>(),
                    Factory.GetInstance<IMediaFileSystem>(),
                    ShortStringHelper,
                    Factory.GetInstance<UmbracoMapper>(),
                    Factory.GetInstance<IContentSettings>(),
                    Factory.GetInstance<IHostingEnvironment>(),
                    Factory.GetInstance<IImageUrlGenerator>(),
                    Factory.GetInstance<IPublishedUrlProvider>(),
                    Factory.GetInstance<ISecuritySettings>(),
                    Factory.GetInstance<IRequestAccessor>()

                );
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

            var runner = new TestRunner(CtrlFactory);
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
                new UserMapper(new Lazy<ISqlContext>(() => Current.SqlContext), new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>())
            });

            Mock.Get(Current.SqlContext)
                .Setup(x => x.Mappers)
                .Returns(mappers);
        }

        [Test]
        public async Task GetPagedUsers_Empty()
        {
            ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor)
            {
                var usersController = new UsersController(
                    Factory.GetInstance<IGlobalSettings>(),
                    umbracoContextAccessor,
                    Factory.GetInstance<ISqlContext>(),
                    Factory.GetInstance<ServiceContext>(),
                    Factory.GetInstance<AppCaches>(),
                    Factory.GetInstance<IProfilingLogger>(),
                    Factory.GetInstance<IRuntimeState>(),
                    Factory.GetInstance<IMediaFileSystem>(),
                    ShortStringHelper,
                    Factory.GetInstance<UmbracoMapper>(),
                    Factory.GetInstance<IContentSettings>(),
                    Factory.GetInstance<IHostingEnvironment>(),
                    Factory.GetInstance<IImageUrlGenerator>(),
                    Factory.GetInstance<IPublishedUrlProvider>(),
                    Factory.GetInstance<ISecuritySettings>(),
                    Factory.GetInstance<IRequestAccessor>()
                );
                return usersController;
            }

            MockForGetPagedUsers();

            var runner = new TestRunner(CtrlFactory);
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(response.Item2);
            Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async Task GetPagedUsers_10()
        {
            ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor)
            {
                //setup some mocks
                var userServiceMock = Mock.Get(ServiceContext.UserService);
                var users = MockedUser.CreateMulipleUsers(10);
                long outVal = 10;
                userServiceMock.Setup(service => service.GetAll(
                        It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(),
                        It.IsAny<UserState[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<IQuery<IUser>>()))
                    .Returns(() => users);

                var usersController = new UsersController(
                    Factory.GetInstance<IGlobalSettings>(),
                    umbracoContextAccessor,
                    Factory.GetInstance<ISqlContext>(),
                    Factory.GetInstance<ServiceContext>(),
                    Factory.GetInstance<AppCaches>(),
                    Factory.GetInstance<IProfilingLogger>(),
                    Factory.GetInstance<IRuntimeState>(),
                    Factory.GetInstance<IMediaFileSystem>(),
                    ShortStringHelper,
                    Factory.GetInstance<UmbracoMapper>(),
                    Factory.GetInstance<IContentSettings>(),
                    Factory.GetInstance<IHostingEnvironment>(),
                    Factory.GetInstance<IImageUrlGenerator>(),
                    Factory.GetInstance<IPublishedUrlProvider>(),
                    Factory.GetInstance<ISecuritySettings>(),
                    Factory.GetInstance<IRequestAccessor>()
                );
                return usersController;
            }

            MockForGetPagedUsers();

            var runner = new TestRunner(CtrlFactory);
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(response.Item2);
            Assert.AreEqual(10, obj.TotalItems);
            Assert.AreEqual(10, obj.Items.Count());
        }

        [Test]
        public async Task GetPagedUsers_Fips()
        {
            await RunFipsTest("GetPagedUsers", mock =>
            {
                var users = MockedUser.CreateMulipleUsers(10);
                long outVal = 10;
                mock.Setup(service => service.GetAll(
                        It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(),
                        It.IsAny<UserState[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<IQuery<IUser>>()))
                    .Returns(() => users);
            }, response =>
            {
                var obj = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(response.Item2);
                Assert.AreEqual(10, obj.TotalItems);
                Assert.AreEqual(10, obj.Items.Count());
            });
        }

        [Test]
        public async Task GetById_Fips()
        {
            const int mockUserId = 1234;
            var user = MockedUser.CreateUser();

            await RunFipsTest("GetById", mock =>
            {
                mock.Setup(service => service.GetUserById(1234))
                    .Returns((int i) => i == mockUserId ? user : null);
            }, response =>
            {
                var obj = JsonConvert.DeserializeObject<UserDisplay>(response.Item2);
                Assert.AreEqual(user.Username, obj.Username);
                Assert.AreEqual(user.Email, obj.Email);
            }, new { controller = "Users", action = "GetById" }, $"Users/GetById/{mockUserId}");
        }


        private async Task RunFipsTest(string action, Action<Mock<IUserService>> userServiceSetup,
            Action<Tuple<HttpResponseMessage, string>> verification,
            object routeDefaults = null, string url = null)
        {
            ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor)
            {
                //setup some mocks
                var userServiceMock = Mock.Get(ServiceContext.UserService);
                userServiceSetup(userServiceMock);

                var usersController = new UsersController(
                    Factory.GetInstance<IGlobalSettings>(),
                    umbracoContextAccessor,
                    Factory.GetInstance<ISqlContext>(),
                    Factory.GetInstance<ServiceContext>(),
                    Factory.GetInstance<AppCaches>(),
                    Factory.GetInstance<IProfilingLogger>(),
                    Factory.GetInstance<IRuntimeState>(),
                    Factory.GetInstance<IMediaFileSystem>(),
                    ShortStringHelper,
                    Factory.GetInstance<UmbracoMapper>(),
                    Factory.GetInstance<IContentSettings>(),
                    Factory.GetInstance<IHostingEnvironment>(),
                    Factory.GetInstance<IImageUrlGenerator>(),
                    Factory.GetInstance<IPublishedUrlProvider>(),
                    Factory.GetInstance<ISecuritySettings>(),
                    Factory.GetInstance<IRequestAccessor>()
                );
                return usersController;
            }

            // Testing what happens if the system were configured to only use FIPS-compliant algorithms
            var typ = typeof(CryptoConfig);
            var flds = typ.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            var haveFld = flds.FirstOrDefault(f => f.Name == "s_haveFipsAlgorithmPolicy");
            var isFld = flds.FirstOrDefault(f => f.Name == "s_fipsAlgorithmPolicy");
            var originalFipsValue = CryptoConfig.AllowOnlyFipsAlgorithms;

            try
            {
                if (!originalFipsValue)
                {
                    haveFld.SetValue(null, true);
                    isFld.SetValue(null, true);
                }

                MockForGetPagedUsers();

                var runner = new TestRunner(CtrlFactory);
                var response = await runner.Execute("Users", action, HttpMethod.Get, routeDefaults: routeDefaults, url: url);
                verification(response);
            }
            finally
            {
                if (!originalFipsValue)
                {
                    haveFld.SetValue(null, false);
                    isFld.SetValue(null, false);
                }
            }
        }

        [Test]
        public async Task PostUnlockUsers_When_UserIds_Not_Supplied_Expect_Ok_Response()
        {
            var usersController = CreateSut();

            usersController.Request = new HttpRequestMessage();

            var response = await usersController.PostUnlockUsers(new int[0]);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void PostUnlockUsers_When_User_Does_Not_Exist_Expect_InvalidOperationException()
        {
            var mockUserManager = CreateMockUserManager();
            var usersController = CreateSut(mockUserManager);

            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((BackOfficeIdentityUser) null);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await usersController.PostUnlockUsers(new[] {1}));
        }

        [Test]
        public async Task PostUnlockUsers_When_User_Lockout_Update_Fails_Expect_Failure_Response()
        {
            var mockUserManager = CreateMockUserManager();
            var usersController = CreateSut(mockUserManager);

            const string expectedMessage = "identity error!";
            var user = new BackOfficeIdentityUser(
                new Mock<IGlobalSettings>().Object,
                1,
                new List<IReadOnlyUserGroup>())
            {
                Name = "bob"
            };

            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = expectedMessage}));

            var response = await usersController.PostUnlockUsers(new[] { 1 });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("X-Status-Reason", out var values));
            Assert.True(values.Contains("Validation failed"));

            var responseContent = response.Content as ObjectContent<HttpError>;
            var responseValue = responseContent?.Value as HttpError;
            Assert.NotNull(responseValue);
            Assert.True(responseValue.Message.Contains(expectedMessage));
            Assert.True(responseValue.Message.Contains(user.Id.ToString()));
        }

        [Test]
        public async Task PostUnlockUsers_When_One_UserId_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
        {
            var mockUserManager = CreateMockUserManager();
            var usersController = CreateSut(mockUserManager);

            var user = new BackOfficeIdentityUser(
                new Mock<IGlobalSettings>().Object,
                1,
                new List<IReadOnlyUserGroup>())
            {
                Name = "bob"
            };

            mockUserManager.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var response = await usersController.PostUnlockUsers(new[] { user.Id });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = response.Content as ObjectContent<SimpleNotificationModel>;
            var notifications = responseContent?.Value as SimpleNotificationModel;
            Assert.NotNull(notifications);
            Assert.AreEqual(user.Name, notifications.Message);
            mockUserManager.Verify();
        }

        [Test]
        public async Task PostUnlockUsers_When_Multiple_UserIds_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
        {
            var mockUserManager = CreateMockUserManager();
            var usersController = CreateSut(mockUserManager);

            var user1 = new BackOfficeIdentityUser(
                new Mock<IGlobalSettings>().Object,
                1,
                new List<IReadOnlyUserGroup>())
            {
                Name = "bob"
            };
            var user2 = new BackOfficeIdentityUser(
                new Mock<IGlobalSettings>().Object,
                2,
                new List<IReadOnlyUserGroup>())
            {
                Name = "alice"
            };
            var userIdsToLock = new[] {user1.Id, user2.Id};

            mockUserManager.Setup(x => x.FindByIdAsync(user1.Id.ToString()))
                .ReturnsAsync(user1);
            mockUserManager.Setup(x => x.FindByIdAsync(user2.Id.ToString()))
                .ReturnsAsync(user2);
            mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user1, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();
            mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user2, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var response = await usersController.PostUnlockUsers(userIdsToLock);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = response.Content as ObjectContent<SimpleNotificationModel>;
            var notifications = responseContent?.Value as SimpleNotificationModel;
            Assert.NotNull(notifications);
            Assert.AreEqual(userIdsToLock.Length.ToString(), notifications.Message);
            mockUserManager.Verify();
        }

        private UsersController CreateSut(IMock<BackOfficeUserManager<BackOfficeIdentityUser>> mockUserManager = null)
        {
            var mockLocalizedTextService = new Mock<ILocalizedTextService>();
            mockLocalizedTextService.Setup(x => x.Localize(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>()))
                .Returns((string key, CultureInfo ci, IDictionary<string, string> tokens)
                    => tokens.Aggregate("", (current, next) => current + (current == string.Empty ? "" : ",") + next.Value));

            var usersController = new UsersController(
                Factory.GetInstance<IGlobalSettings>(),
                Factory.GetInstance<IUmbracoContextAccessor>(),
                Factory.GetInstance<ISqlContext>(),
                ServiceContext.CreatePartial(localizedTextService: mockLocalizedTextService.Object),
                Factory.GetInstance<AppCaches>(),
                Factory.GetInstance<IProfilingLogger>(),
                Factory.GetInstance<IRuntimeState>(),
                Factory.GetInstance<IMediaFileSystem>(),
                ShortStringHelper,
                Factory.GetInstance<UmbracoMapper>(),
                Factory.GetInstance<IContentSettings>(),
                Factory.GetInstance<IHostingEnvironment>(),
                Factory.GetInstance<IImageUrlGenerator>(),
                Factory.GetInstance<IPublishedUrlProvider>(),
                Factory.GetInstance<ISecuritySettings>(),
                Factory.GetInstance<IRequestAccessor>());

            var mockOwinContext = new Mock<IOwinContext>();
            var mockUserManagerMarker = new Mock<IBackOfficeUserManagerMarker>();

            mockOwinContext.Setup(x => x.Get<IBackOfficeUserManagerMarker>(It.IsAny<string>()))
                .Returns(mockUserManagerMarker.Object);
            mockUserManagerMarker.Setup(x => x.GetManager(It.IsAny<IOwinContext>()))
                .Returns(mockUserManager?.Object ?? CreateMockUserManager().Object);

            usersController.Request = new HttpRequestMessage();
            usersController.Request.Properties["MS_OwinContext"] = mockOwinContext.Object;
            usersController.Request.Properties[HttpPropertyKeys.RequestContextKey] = new HttpRequestContext {Configuration = new HttpConfiguration()};

            return usersController;
        }

        private static Mock<BackOfficeUserManager<BackOfficeIdentityUser>> CreateMockUserManager()
        {
            return new Mock<BackOfficeUserManager<BackOfficeIdentityUser>>(
                new Mock<IPasswordConfiguration>().Object,
                new Mock<IIpResolver>().Object,
                new Mock<IUserStore<BackOfficeIdentityUser>>().Object,
                null, null, null, null, null, null, null);
        }
    }
}
