using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class ResetPasswordControllerTests : ManagementApiUserGroupTestBase<ResetPasswordController>
{
    private const string RegisteredEmail = "registered@umbraco.com";
    private const string UnknownEmail = "unknown@umbraco.com";

    private readonly Mock<IUserForgotPasswordSender> _senderMock = new();

    protected override Expression<Func<ResetPasswordController, object>> MethodSelector => x => x.RequestPasswordReset(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Password reset is not configured unless a test opts in, which is what the user group tests expect.
        _senderMock.Reset();
        _senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(false);
        builder.Services.AddUnique(_senderMock.Object);
        builder.Services.Configure<WebRoutingSettings>(x => x.UmbracoApplicationUrl = "https://localhost/");
    }

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ResetPasswordRequestModel resetPasswordRequestModel = new() { Email = UserEmail };

        return await Client.PostAsync(Url, JsonContent.Create(resetPasswordRequestModel));
    }

    [Test]
    public async Task Unavailable_Password_Reset_Returns_Same_Problem_Details_For_Registered_And_Unknown_Email()
    {
        _senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(true);
        _senderMock.Setup(x => x.IsPasswordResetAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await EnsureRegisteredUser();

        HttpResponseMessage registeredResponse = await RequestPasswordReset(RegisteredEmail);
        HttpResponseMessage unknownResponse = await RequestPasswordReset(UnknownEmail);

        await AssertPasswordResetUnavailable(registeredResponse);
        await AssertPasswordResetUnavailable(unknownResponse);
    }

    [Test]
    public async Task Failed_Send_Returns_Same_Ok_Response_For_Registered_And_Unknown_Email()
    {
        _senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(true);
        _senderMock.Setup(x => x.IsPasswordResetAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _senderMock
            .Setup(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()))
            .ThrowsAsync(new SocketException());
        await EnsureRegisteredUser();

        HttpResponseMessage registeredResponse = await RequestPasswordReset(RegisteredEmail);
        HttpResponseMessage unknownResponse = await RequestPasswordReset(UnknownEmail);

        Assert.AreEqual(HttpStatusCode.OK, registeredResponse.StatusCode, await registeredResponse.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.OK, unknownResponse.StatusCode, await unknownResponse.Content.ReadAsStringAsync());
        _senderMock.Verify(x => x.SendForgotPassword(It.IsAny<UserForgotPasswordMessage>()), Times.Once);
    }

    private async Task<HttpResponseMessage> RequestPasswordReset(string email)
        => await Client.PostAsync(Url, JsonContent.Create(new ResetPasswordRequestModel { Email = email }));

    private static async Task AssertPasswordResetUnavailable(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, body);

        using var problemDetails = JsonDocument.Parse(body);
        Assert.AreEqual(
            nameof(UserOperationStatus.PasswordResetUnavailable),
            problemDetails.RootElement.GetProperty("operationStatus").GetString());
    }

    // The database is shared across the fixture, so the user may already exist from an earlier test.
    private async Task EnsureRegisteredUser()
    {
        IUserService userService = GetRequiredService<IUserService>();
        if (userService.GetByEmail(RegisteredEmail) is not null)
        {
            return;
        }

        var userGroup = await GetRequiredService<IUserGroupService>().GetAsync(Constants.Security.AdminGroupAlias);
        var createModel = new UserCreateModel
        {
            UserName = RegisteredEmail,
            Email = RegisteredEmail,
            Name = "Registered User",
            UserGroupKeys = new HashSet<Guid> { userGroup!.Key },
        };

        var result = await userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(result.Success);
    }
}
