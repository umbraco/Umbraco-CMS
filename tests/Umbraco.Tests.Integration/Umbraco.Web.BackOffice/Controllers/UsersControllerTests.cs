// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class UsersControllerTests : UmbracoTestServerTestBase
{
    [Test]
    public async Task Save_User()
    {
        var url = PrepareApiControllerUrl<UsersController>(x => x.PostSaveUser(null));

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
        var response = await Client.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(userSave), Encoding.UTF8, MediaTypeNames.Application.Json));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            var actual = JsonConvert.DeserializeObject<UserDisplay>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });
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
        // We get page 2 to force an empty response because there always in the useradmin user
        var url = PrepareApiControllerUrl<UsersController>(x =>
            x.GetPagedUsers(2, 10, "username", Direction.Ascending, null, null, string.Empty));

        // Act
        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var actual = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.TotalItems);
            CollectionAssert.IsEmpty(actual.Items);
        });
    }

    [Test]
    public async Task GetPagedUsers_multiple_pages()
    {
        var totalNumberOfUsers = 11;
        var pageSize = totalNumberOfUsers - 1;
        var url = PrepareApiControllerUrl<UsersController>(x =>
            x.GetPagedUsers(1, pageSize, "username", Direction.Ascending, null, null, string.Empty));

        var userService = GetRequiredService<IUserService>();

        // We already has admin user = -1, so we start from 1.
        for (var i = 1; i < totalNumberOfUsers; i++)
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
        var actual = JsonConvert.DeserializeObject<PagedResult<UserBasic>>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });
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
        var url = PrepareApiControllerUrl<UsersController>(x => x.PostUnlockUsers(Array.Empty<int>()));

        // Act
        var response = await Client.PostAsync(url, new StringContent(string.Empty));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task PostUnlockUsers_When_User_Does_Not_Exist_Expect_Zero_Users_Message()
    {
        var userId = 42; // Must not exist
        var url = PrepareApiControllerUrl<UsersController>(x => x.PostUnlockUsers(new[] { userId }));

        // Act
        var response = await Client.PostAsync(url, new StringContent(string.Empty));
        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var actual = JsonConvert.DeserializeObject<SimpleNotificationModel>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });
        Assert.Multiple(() => Assert.AreEqual("Unlocked 0 users", actual.Message));
    }

    [Test]
    public async Task PostUnlockUsers_When_One_UserId_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
    {
        var userService = GetRequiredService<IUserService>();

        var user = new UserBuilder()
            .AddUserGroup()
            .WithAlias("writer") // Needs to be an existing alias
            .Done()
            .WithIsLockedOut(true, DateTime.UtcNow)
            .Build();

        userService.Save(user);
        var url = PrepareApiControllerUrl<UsersController>(x => x.PostUnlockUsers(new[] { user.Id }));

        // Act
        var response = await Client.PostAsync(url, new StringContent(string.Empty));
        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var actual = JsonConvert.DeserializeObject<SimpleNotificationModel>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });

        Assert.Multiple(() =>
        {
            Assert.NotNull(actual);
            Assert.AreEqual($"{user.Name} is now unlocked", actual.Message);
        });
    }

    [Test]
    public async Task
        PostUnlockUsers_When_Multiple_UserIds_Supplied_Expect_User_Locked_Out_With_Correct_Response_Message()
    {
        var numberOfUsers = 3;
        var userService = GetRequiredService<IUserService>();

        var users = new List<IUser>();
        for (var i = 0; i < numberOfUsers; i++)
        {
            users.Add(new UserBuilder()
                .WithName($"Test User {i}")
                .WithEmail($"TestUser{i}@umbraco.com")
                .WithUsername($"TestUser{i}")
                .AddUserGroup()
                .WithAlias("writer") // Needs to be an existing alias
                .Done()
                .WithIsLockedOut(true, DateTime.UtcNow)
                .Build());
        }

        foreach (var user in users)
        {
            userService.Save(user);
        }

        var url = PrepareApiControllerUrl<UsersController>(x => x.PostUnlockUsers(users.Select(x => x.Id).ToArray()));

        // Act
        var response = await Client.PostAsync(url, new StringContent(string.Empty));
        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var actual = JsonConvert.DeserializeObject<SimpleNotificationModel>(body, new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });

        Assert.Multiple(() =>
        {
            Assert.NotNull(actual);
            Assert.AreEqual($"Unlocked {users.Count()} users", actual.Message);
        });
    }
}
