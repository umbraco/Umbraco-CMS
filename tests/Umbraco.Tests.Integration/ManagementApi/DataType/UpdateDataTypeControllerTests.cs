using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class UpdateDataTypeControllerTests : ManagementApiTest<UpdateDataTypeController>
{
    protected override Expression<Func<UpdateDataTypeController, object>> MethodSelector =>
        x => x.Update(Guid.NewGuid(), null);

    private readonly List<HttpStatusCode> _authenticatedStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await SendUpdateDataTypeRequestAsync("admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await SendUpdateDataTypeRequestAsync("editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await SendUpdateDataTypeRequestAsync("sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await SendUpdateDataTypeRequestAsync("translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await SendUpdateDataTypeRequestAsync("writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var updateDataTypeModel = GenerateUpdateDataTypeRequestModel();

        var response = await Client.PutAsync(Url, JsonContent.Create(updateDataTypeModel));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> SendUpdateDataTypeRequestAsync(string userEmail, string userPassword, Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, userEmail, userPassword, userGroupKey);

        var updateDataTypeModel = GenerateUpdateDataTypeRequestModel();

        return await Client.PutAsync(Url, JsonContent.Create(updateDataTypeModel));
    }

    private UpdateDataTypeRequestModel GenerateUpdateDataTypeRequestModel()
    {
        return new UpdateDataTypeRequestModel
        {
        };
    }

}
