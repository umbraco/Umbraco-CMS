// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Web.BackOffice.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Authorization;

public class AdminUsersHandlerTests
{
    private const string SingleUserEditQueryStringName = "id";
    private const string MultipleUserEditQueryStringName = "ids";

    private const int Admin1UserId = 0;
    private const int Admin2UserId = 1;
    private const int NonAdmin1UserId = 2;
    private const int NonAdmin2UserId = 3;
    private const int NonAdmin3UserId = 4;

    [Test]
    public async Task Missing_QueryString_Value_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var sut = CreateHandler("xxx");

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Non_Integer_QueryString_Value_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var sut = CreateHandler(queryStringValue: "xxx");

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Single_Admin_User_By_Admin_User_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var sut = CreateHandler(
            queryStringValue: Admin2UserId.ToString(CultureInfo.InvariantCulture),
            editingWithAdmin: true);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Single_Admin_User_By_Non_Admin_User_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var sut = CreateHandler(queryStringValue: Admin2UserId.ToString(CultureInfo.InvariantCulture));

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Single_Non_Admin_User_By_Non_Admin_User_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext();
        var sut = CreateHandler(queryStringValue: NonAdmin2UserId.ToString(CultureInfo.InvariantCulture));

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Multiple_Users_Including_Admins_By_Admin_User_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(MultipleUserEditQueryStringName);
        var sut = CreateHandler(MultipleUserEditQueryStringName, $"{Admin2UserId},{NonAdmin2UserId}", true);

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Multiple_Users_Including_Admins_By_Non_Admin_User_Is_Not_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(MultipleUserEditQueryStringName);
        var sut = CreateHandler(MultipleUserEditQueryStringName, $"{Admin2UserId},{NonAdmin2UserId}");

        await sut.HandleAsync(authHandlerContext);

        Assert.IsFalse(authHandlerContext.HasSucceeded);
    }

    [Test]
    public async Task Editing_Multiple_Users_Not_Including_Admins_By_Non_Admin_User_Is_Authorized()
    {
        var authHandlerContext = CreateAuthorizationHandlerContext(MultipleUserEditQueryStringName);
        var sut = CreateHandler(MultipleUserEditQueryStringName, $"{NonAdmin2UserId},{NonAdmin3UserId}");

        await sut.HandleAsync(authHandlerContext);

        Assert.IsTrue(authHandlerContext.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(
        string queryStringName = SingleUserEditQueryStringName)
    {
        var requirement = new AdminUsersRequirement(queryStringName);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));
        var resource = new object();
        return new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, user, resource);
    }

    private AdminUsersHandler CreateHandler(
        string queryStringName = SingleUserEditQueryStringName,
        string queryStringValue = "",
        bool editingWithAdmin = false)
    {
        var mockHttpContextAccessor = CreateMockHttpContextAccessor(queryStringName, queryStringValue);
        CreateMockUserServiceAndSecurityAccessor(editingWithAdmin, out var mockUserService, out var mockBackOfficeSecurityAccessor);
        var userEditorAuthorizationHelper = CreateUserEditorAuthorizationHelper();
        return new AdminUsersHandler(
            mockHttpContextAccessor.Object,
            mockUserService.Object,
            mockBackOfficeSecurityAccessor.Object,
            userEditorAuthorizationHelper);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string queryStringName, string queryStringValue)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockHttpRequest = new Mock<HttpRequest>();
        var queryParams = new Dictionary<string, StringValues> { { queryStringName, queryStringValue } };
        mockHttpRequest.SetupGet(x => x.Query).Returns(new QueryCollection(queryParams));
        mockHttpContext.SetupGet(x => x.Request).Returns(mockHttpRequest.Object);
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
        return mockHttpContextAccessor;
    }

    private static void CreateMockUserServiceAndSecurityAccessor(
        bool editingWithAdmin,
        out Mock<IUserService> mockUserService,
        out Mock<IBackOfficeSecurityAccessor> mockBackOfficeSecurityAccessor)
    {
        mockUserService = new Mock<IUserService>();
        var adminUser1 = CreateUser(Admin1UserId, mockUserService, true);
        var adminUser2 = CreateUser(Admin2UserId, mockUserService, true);
        var nonAdminUser1 = CreateUser(NonAdmin1UserId, mockUserService);
        var nonAdminUser2 = CreateUser(NonAdmin2UserId, mockUserService);
        var nonAdminUser3 = CreateUser(NonAdmin3UserId, mockUserService);

        // Single user requests have been setup in the create user operations, but
        // we also need to mock the responses when multiple users are being editing.
        mockUserService
            .Setup(x => x.GetUsersById(It.Is<int[]>(y =>
                y.Length == 2 && y[0] == Admin2UserId && y[1] == NonAdmin2UserId)))
            .Returns(new List<IUser> { adminUser2, nonAdminUser2 });
        mockUserService
            .Setup(x => x.GetUsersById(It.Is<int[]>(y =>
                y.Length == 2 && y[0] == NonAdmin2UserId && y[1] == NonAdmin3UserId)))
            .Returns(new List<IUser> { nonAdminUser2, nonAdminUser3 });

        var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
        mockBackOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(editingWithAdmin ? adminUser1 : nonAdminUser1);
        mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);
    }

    private static User CreateUser(int id, Mock<IUserService> mockUserService, bool isAdmin = false)
    {
        var user = new UserBuilder()
            .WithId(id)
            .AddUserGroup()
            .WithAlias(isAdmin ? Constants.Security.AdminGroupAlias : Constants.Security.EditorGroupAlias)
            .Done()
            .Build();

        mockUserService
            .Setup(x => x.GetUsersById(It.Is<int[]>(y => y.Length == 1 && y[0] == id)))
            .Returns(new List<IUser> { user });

        return user;
    }

    private static UserEditorAuthorizationHelper CreateUserEditorAuthorizationHelper()
    {
        var mockContentService = new Mock<IContentService>();
        var mockMediaService = new Mock<IMediaService>();
        var mockEntityService = new Mock<IEntityService>();
        return new UserEditorAuthorizationHelper(
            mockContentService.Object,
            mockMediaService.Object,
            mockEntityService.Object,
            AppCaches.Disabled);
    }
}
