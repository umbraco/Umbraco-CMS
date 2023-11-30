using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class MoveDataTypeControllerTests : ManagementApiTest<MoveDataTypeController>
{
    protected override Expression<Func<MoveDataTypeController, object>> MethodSelector =>
        x => x.Move(Guid.NewGuid(), null);

    private readonly List<HttpStatusCode> _authenticatedStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.OK,
            HttpStatusCode.NotFound
        };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await SendMoveDataTypeRequestAsync("admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await SendMoveDataTypeRequestAsync("editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await SendMoveDataTypeRequestAsync("sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await SendMoveDataTypeRequestAsync("translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await SendMoveDataTypeRequestAsync("writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var moveDataTypeModel = GenerateMoveDataTypeRequestModel();

        var response = await Client.PostAsync(Url, JsonContent.Create(moveDataTypeModel));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> SendMoveDataTypeRequestAsync(string userEmail, string userPassword, Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, userEmail, userPassword, userGroupKey);

        var moveDataTypeModel = GenerateMoveDataTypeRequestModel();

        return await Client.PostAsync(Url, JsonContent.Create(moveDataTypeModel));
    }

    private MoveDataTypeRequestModel GenerateMoveDataTypeRequestModel()
    {
        return new MoveDataTypeRequestModel
        {
            TargetId = Guid.NewGuid()
        };
    }

}
