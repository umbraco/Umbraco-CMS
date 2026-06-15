using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

public abstract class ManagementApiUserGroupTestBase<T> : ManagementApiTest<T>
    where T : ManagementApiControllerBase
{
    protected virtual string UserEmail => "test@umbraco.com";

    protected const string UserPassword = "1234567890";

    protected override Expression<Func<T, object>> MethodSelector { get; set; }

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
        var response = await AuthorizedRequest(Constants.Security.AdminGroupKey, "Admin");
        Assert.That(response.StatusCode, Is.EqualTo(AdminUserGroupAssertionModel.ExpectedStatusCode), await response.Content.ReadAsStringAsync());
    }

    // Editor
    [Test]
    public virtual async Task As_Editor_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.EditorGroupKey, "Editor");
        Assert.That(
            response.StatusCode, Is.EqualTo(EditorUserGroupAssertionModel.ExpectedStatusCode),
            await response.Content.ReadAsStringAsync());
    }

    // SensitiveData
    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.SensitiveDataGroupKey, "SensitiveData");
        Assert.That(
            response.StatusCode, Is.EqualTo(SensitiveDataUserGroupAssertionModel.ExpectedStatusCode),
            await response.Content.ReadAsStringAsync());
    }

    // Translator
    [Test]
    public virtual async Task As_Translator_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.TranslatorGroupKey, "Translator");
        Assert.That(
            response.StatusCode, Is.EqualTo(TranslatorUserGroupAssertionModel.ExpectedStatusCode),
            await response.Content.ReadAsStringAsync());
    }

    // Writer
    [Test]
    public virtual async Task As_Writer_I_Have_Specified_Access()
    {
        var response = await AuthorizedRequest(Constants.Security.WriterGroupKey, "Writer");
        Assert.That(
            response.StatusCode, Is.EqualTo(WriterUserGroupAssertionModel.ExpectedStatusCode),
            await response.Content.ReadAsStringAsync());
    }

    // Unauthorized
    [Test]
    public virtual async Task As_Unauthorized_I_Have_Specified_Access()
    {
        var response = await ClientRequest();
        Assert.That(
            response.StatusCode, Is.EqualTo(UnauthorizedUserGroupAssertionModel.ExpectedStatusCode),
            await response.Content.ReadAsStringAsync());
    }

    protected virtual async Task<HttpResponseMessage> AuthorizedRequest(Guid userGroupKey, string groupName)
    {
        await AuthenticateUser(userGroupKey, groupName);
        return await ClientRequest();
    }

    protected virtual async Task AuthenticateUser(Guid userGroupKey, string groupName)
    {
        await AuthenticateClientAsync(Client, UserEmail + groupName, UserPassword, userGroupKey);
    }

    protected virtual async Task<HttpResponseMessage> ClientRequest()
    {
        return await Client.GetAsync(Url);
    }

    protected class UserGroupAssertionModel
    {
        public HttpStatusCode ExpectedStatusCode { get; set; }
    }
}
