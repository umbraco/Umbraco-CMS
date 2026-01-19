using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeControllerTests : ManagementApiUserGroupTestBase<BackOfficeController>
{
    protected override Expression<Func<BackOfficeController, object>> MethodSelector =>
        x => x.Login(CancellationToken.None, null);

    // Admin
    [Test]
    public override async Task As_Admin_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.AdminGroupKey, "Admin");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Editor
    [Test]
    public override async Task As_Editor_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.EditorGroupKey, "Editor");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // SensitiveData
    [Test]
    public override async Task As_Sensitive_Data_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.SensitiveDataGroupKey, "SensitiveData");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Translator
    [Test]
    public override async Task As_Translator_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.TranslatorGroupKey, "Translator");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Writer
    [Test]
    public override async Task As_Writer_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.WriterGroupKey, "Writer");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Unauthorized
    [Test]
    public override async Task As_Unauthorized_I_Have_Specified_Access()
    {
        var response = await ClientRequest();
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        LoginRequestModel loginRequestModel = new() { Username = UserEmail, Password = UserPassword };

        return await Client.PostAsync(Url, JsonContent.Create(loginRequestModel));
    }

    protected override async Task AuthenticateUser(Guid userGroupKey, string groupName) =>
        await AuthenticateClientAsync(Client, UserEmail, UserPassword, userGroupKey);
}
