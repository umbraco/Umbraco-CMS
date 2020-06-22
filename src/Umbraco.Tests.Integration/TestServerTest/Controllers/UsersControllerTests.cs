using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Testing;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.Formatters;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Integration.TestServerTest.Controllers
{
  //  [Explicit("We need to fix the tests on buildserver and when running multiple tests in one run")]
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UsersControllerTests : UmbracoTestServerTestBase
    {
        [Test]
        public async Task Save_User()
        {
            var url = PrepareUrl<UsersController>(x => x.PostSaveUser(null));

            var userService = GetRequiredService<IUserService>();

            var user = new UserBuilder()
                .AddUserGroup()
                .WithAlias("writer") // Needs to be an existing alias
                .Done()
                .Build();

            userService.Save(user);

            var userSave = new UserSave
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Culture = "en",
                Name = user.Name,
                UserGroups = user.Groups.Select(x => x.Alias).ToArray()
            };
            // Act
            var response = await Client.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(userSave), Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            // Assert

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
                var actual = JsonConvert.DeserializeObject<UserDisplay>(body, new JsonSerializerSettings
                {
                    ContractResolver = new IgnoreRequiredAttributsResolver()
                });
                Assert.AreEqual(userSave.Name, actual.Name);
                Assert.AreEqual(userSave.Id, actual.Id);
                Assert.AreEqual(userSave.Email, actual.Email);
                var userGroupAliases = actual.UserGroups.Select(x => x.Alias).ToArray();
                CollectionAssert.AreEquivalent(userSave.UserGroups, userGroupAliases);
            });
        }

        [Test]
         public async Task GetPagedUsers_Empty()
         {
             //We get page 2 to force an empty response because there always in the useradmin user
             var url = PrepareUrl<UsersController>(x => x.GetPagedUsers(2, 10, "username", Direction.Ascending, null, null, string.Empty));

             // Act
             var response = await Client.GetAsync(url);

             var body = await response.Content.ReadAsStringAsync();
             body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
             Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
             var actual = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(body, new JsonSerializerSettings
             {
                 ContractResolver = new IgnoreRequiredAttributsResolver()
             });
             Assert.Multiple(() =>
             {
                 Assert.IsNotNull(actual);
                 Assert.AreEqual(1, actual.TotalItems);
                 CollectionAssert.IsEmpty(actual.Items);
             });
         }

         [Test]
         public async Task GetPagedUsers_11()
         {
             var totalNumberOfUsers = 11;
             var pageSize = totalNumberOfUsers - 1;
             var url = PrepareUrl<UsersController>(x => x.GetPagedUsers(1, pageSize, "username", Direction.Ascending, null, null, string.Empty));

             var userService = GetRequiredService<IUserService>();

             for (int i = 1; i < totalNumberOfUsers; i++) // We already has admin user = -1, so we start from 1
             {
                 var user = new UserBuilder()
                     .WithName($"Test user {i}")
                     .AddUserGroup()
                     .WithAlias("writer") // Needs to be an existing alias
                     .Done()
                     .Build();

                 userService.Save(user);
             }

             // Act
             var response = await Client.GetAsync(url);

             var body = await response.Content.ReadAsStringAsync();
             body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
             Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
             var actual = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(body, new JsonSerializerSettings
             {
                 ContractResolver = new IgnoreRequiredAttributsResolver()
             });
             Assert.Multiple(() =>
             {
                 Assert.IsNotNull(actual);
                 Assert.AreEqual(totalNumberOfUsers, actual.TotalItems);
                 Assert.AreEqual(pageSize, actual.Items.Count());
             });
         }

         [Test]
        public async Task PostUnlockUsers_When_UserIds_Not_Supplied_Expect_Ok_Response()
        {
            var url = PrepareUrl<UsersController>(x => x.PostUnlockUsers(Array.Empty<int>()));

            // Act
            var response = await Client.PostAsync(url, new StringContent(string.Empty));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task PostUnlockUsers_When_User_Does_Not_Exist_Expect_InvalidOperationException()
        {
            var userId = 42; // Must not exist
            var url = PrepareUrl<UsersController>(x => x.PostUnlockUsers(new []{userId}));

            // Act
            var response = await Client.PostAsync(url, new StringContent(string.Empty));
            var body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

            var actual = JsonConvert.DeserializeObject<ExceptionViewModel>(body, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreRequiredAttributsResolver()
            });
            Assert.Multiple(() =>
            {
                var expected = new InvalidOperationException();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.GetType(), actual.ExceptionType);
                Assert.AreEqual(expected.Message, actual.ExceptionMessage);
            });
        }

        // [Test]
        // public async Task PostUnlockUsers_When_User_Lockout_Update_Fails_Expect_Failure_Response()
        // {
        //     var mockUserManager = CreateMockUserManager();
        //     var usersController = CreateSut(mockUserManager);
        //
        //     const string expectedMessage = "identity error!";
        //     var user = new BackOfficeIdentityUser(
        //         new Mock<IGlobalSettings>().Object,
        //         1,
        //         new List<IReadOnlyUserGroup>())
        //     {
        //         Name = "bob"
        //     };
        //
        //     mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
        //         .ReturnsAsync(user);
        //     mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
        //         .ReturnsAsync(IdentityResult.Failed(new IdentityError {Description = expectedMessage}));
        //
        //     var response = await usersController.PostUnlockUsers(new[] { 1 });
        //
        //     Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        //     Assert.True(response.Headers.TryGetValues("X-Status-Reason", out var values));
        //     Assert.True(values.Contains("Validation failed"));
        //
        //     var responseContent = response.Content as ObjectContent<HttpError>;
        //     var responseValue = responseContent?.Value as HttpError;
        //     Assert.NotNull(responseValue);
        //     Assert.True(responseValue.Message.Contains(expectedMessage));
        //     Assert.True(responseValue.Message.Contains(user.Id.ToString()));
        // }
        //
        // [Test]
        // public async Task PostUnlockUsers_When_One_UserId_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
        // {
        //     var mockUserManager = CreateMockUserManager();
        //     var usersController = CreateSut(mockUserManager);
        //
        //     var user = new BackOfficeIdentityUser(
        //         new Mock<IGlobalSettings>().Object,
        //         1,
        //         new List<IReadOnlyUserGroup>())
        //     {
        //         Name = "bob"
        //     };
        //
        //     mockUserManager.Setup(x => x.FindByIdAsync(user.Id.ToString()))
        //         .ReturnsAsync(user);
        //     mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>()))
        //         .ReturnsAsync(IdentityResult.Success)
        //         .Verifiable();
        //
        //     var response = await usersController.PostUnlockUsers(new[] { user.Id });
        //
        //     Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        //
        //     var responseContent = response.Content as ObjectContent<SimpleNotificationModel>;
        //     var notifications = responseContent?.Value as SimpleNotificationModel;
        //     Assert.NotNull(notifications);
        //     Assert.AreEqual(user.Name, notifications.Message);
        //     mockUserManager.Verify();
        // }
        //
        // [Test]
        // public async Task PostUnlockUsers_When_Multiple_UserIds_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
        // {
        //     var mockUserManager = CreateMockUserManager();
        //     var usersController = CreateSut(mockUserManager);
        //
        //     var user1 = new BackOfficeIdentityUser(
        //         new Mock<IGlobalSettings>().Object,
        //         1,
        //         new List<IReadOnlyUserGroup>())
        //     {
        //         Name = "bob"
        //     };
        //     var user2 = new BackOfficeIdentityUser(
        //         new Mock<IGlobalSettings>().Object,
        //         2,
        //         new List<IReadOnlyUserGroup>())
        //     {
        //         Name = "alice"
        //     };
        //     var userIdsToLock = new[] {user1.Id, user2.Id};
        //
        //     mockUserManager.Setup(x => x.FindByIdAsync(user1.Id.ToString()))
        //         .ReturnsAsync(user1);
        //     mockUserManager.Setup(x => x.FindByIdAsync(user2.Id.ToString()))
        //         .ReturnsAsync(user2);
        //     mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user1, It.IsAny<DateTimeOffset>()))
        //         .ReturnsAsync(IdentityResult.Success)
        //         .Verifiable();
        //     mockUserManager.Setup(x => x.SetLockoutEndDateAsync(user2, It.IsAny<DateTimeOffset>()))
        //         .ReturnsAsync(IdentityResult.Success)
        //         .Verifiable();
        //
        //     var response = await usersController.PostUnlockUsers(userIdsToLock);
        //
        //     Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        //
        //     var responseContent = response.Content as ObjectContent<SimpleNotificationModel>;
        //     var notifications = responseContent?.Value as SimpleNotificationModel;
        //     Assert.NotNull(notifications);
        //     Assert.AreEqual(userIdsToLock.Length.ToString(), notifications.Message);
        //     mockUserManager.Verify();
        // }
        //
         // [Test]
         // public async Task GetPagedUsers_Fips()
         // {
         //     await RunFipsTest("GetPagedUsers", mock =>
         //     {
         //         var users = MockedUser.CreateMulipleUsers(10);
         //         long outVal = 10;
         //         mock.Setup(service => service.GetAll(
         //                 It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(),
         //                 It.IsAny<UserState[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<IQuery<IUser>>()))
         //             .Returns(() => users);
         //     }, response =>
         //     {
         //         var obj = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(response.Item2);
         //         Assert.AreEqual(10, obj.TotalItems);
         //         Assert.AreEqual(10, obj.Items.Count());
         //     });
         // }
//
//         [Test]
//         public async Task GetById_Fips()
//         {
//             const int mockUserId = 1234;
//             var user = MockedUser.CreateUser();
//
//             await RunFipsTest("GetById", mock =>
//             {
//                 mock.Setup(service => service.GetUserById(1234))
//                     .Returns((int i) => i == mockUserId ? user : null);
//             }, response =>
//             {
//                 var obj = JsonConvert.DeserializeObject<UserDisplay>(response.Item2);
//                 Assert.AreEqual(user.Username, obj.Username);
//                 Assert.AreEqual(user.Email, obj.Email);
//             }, new { controller = "Users", action = "GetById" }, $"Users/GetById/{mockUserId}");
//         }
    }
}
