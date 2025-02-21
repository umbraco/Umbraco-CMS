using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

[TestFixture]
public abstract class ManagementApiUserGroupTestBase<T> : ManagementApiTest<T>
    where T : ManagementApiControllerBase
{
    protected const string UserEmail = "test@umbraco.com";
    protected const string UserPassword = "1234567890";

    protected override Expression<Func<T, object>> MethodSelector { get; }

    protected virtual UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected virtual UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected virtual UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected virtual UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected virtual UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected virtual UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    // Admin
    [Test]
    public virtual async Task As_Admin_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.AdminGroupKey);

        Assert.AreEqual(AdminUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Editor
    [Test]
    public virtual async Task As_Editor_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.EditorGroupKey);

        Assert.AreEqual(EditorUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // SensitiveData
    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(SensitiveDataUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Translator
    [Test]
    public virtual async Task As_Translator_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(TranslatorUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Writer
    [Test]
    public virtual async Task As_Writer_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.WriterGroupKey);

        Assert.AreEqual(WriterUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // Unauthorized
    [Test]
    public virtual async Task As_Unauthorized_I_Have_Specified_Access()
    {
        var response = await ClientRequest();

        Assert.AreEqual(UnauthorizedUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    protected virtual async Task<HttpResponseMessage> AuthorizedRequest(Guid userGroupKey)
    {
        await AuthenticateUser(userGroupKey);

        return await ClientRequest();
    }

    protected virtual async Task AuthenticateUser(Guid userGroupKey) => await AuthenticateClientAsync(Client, UserEmail, UserPassword, userGroupKey);

    protected virtual async Task<HttpResponseMessage> ClientRequest() => await Client.GetAsync(Url);
}
