using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class ResetPasswordUnavailableControllerTests : ManagementApiTest<ResetPasswordController>
{
    private const string RegisteredEmail = "registered@umbraco.com";

    protected override Expression<Func<ResetPasswordController, object>> MethodSelector { get; set; } =
        x => x.RequestPasswordReset(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        var senderMock = new Mock<IUserForgotPasswordSender>();
        senderMock.Setup(x => x.IsPasswordResetConfigured()).Returns(true);
        senderMock.Setup(x => x.IsPasswordResetAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        builder.Services.AddUnique(senderMock.Object);
    }

    [Test]
    public async Task Unavailable_Password_Reset_Returns_Same_Problem_Details_For_Registered_And_Unknown_Email()
    {
        await CreateRegisteredUser();

        HttpResponseMessage registeredResponse = await RequestPasswordReset(RegisteredEmail);
        HttpResponseMessage unknownResponse = await RequestPasswordReset("unknown@umbraco.com");

        await AssertPasswordResetUnavailable(registeredResponse);
        await AssertPasswordResetUnavailable(unknownResponse);
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

    private async Task CreateRegisteredUser()
    {
        var userGroup = await GetRequiredService<IUserGroupService>().GetAsync(Constants.Security.AdminGroupAlias);
        var createModel = new UserCreateModel
        {
            UserName = RegisteredEmail,
            Email = RegisteredEmail,
            Name = "Registered User",
            UserGroupKeys = new HashSet<Guid> { userGroup!.Key },
        };

        var result = await GetRequiredService<IUserService>().CreateAsync(Constants.Security.SuperUserKey, createModel, true);
        Assert.IsTrue(result.Success);
    }
}
