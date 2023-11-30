using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

/// <summary>
///
/// </summary>
[TestFixture]
public class CopyDataTypeControllerTests : ManagementApiTest<CopyDataTypeController>
{
    protected override Expression<Func<CopyDataTypeController, object>> MethodSelector =>
        x => x.Copy(Guid.NewGuid(), null);

    private readonly List<HttpStatusCode> _authenticatedStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.Created,
            HttpStatusCode.NotFound
        };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await SendCopyDataTypeRequestAsync("admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await SendCopyDataTypeRequestAsync("editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await SendCopyDataTypeRequestAsync("sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await SendCopyDataTypeRequestAsync("translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await SendCopyDataTypeRequestAsync("writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var copyDataTypeModel = GenerateCopyDataTypeRequestModel();

        var response = await Client.PostAsync(Url, JsonContent.Create(copyDataTypeModel));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> SendCopyDataTypeRequestAsync(string userEmail, string userPassword, Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, userEmail, userPassword, userGroupKey);

        var copyDataTypeModel = GenerateCopyDataTypeRequestModel();

        return await Client.PostAsync(Url, JsonContent.Create(copyDataTypeModel));
    }

    private CopyDataTypeRequestModel GenerateCopyDataTypeRequestModel()
    {
        return new CopyDataTypeRequestModel
        {
            TargetId = Guid.NewGuid()
        };
    }

}
