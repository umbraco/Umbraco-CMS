using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class CreateDataTypeControllerTests : ManagementApiTest<CreateDataTypeController>
{
    protected override Expression<Func<CreateDataTypeController, object>> MethodSelector =>
        x => x.Create(null);

    private readonly List<HttpStatusCode> _authenticatedStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await SendCreateDataTypeRequestAsync("admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await SendCreateDataTypeRequestAsync("editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await SendCreateDataTypeRequestAsync("sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await SendCreateDataTypeRequestAsync("translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await SendCreateDataTypeRequestAsync("writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var createDataTypeModel = GenerateCreateDataTypeRequestModel();

        var response = await Client.PostAsync(Url, JsonContent.Create(createDataTypeModel));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> SendCreateDataTypeRequestAsync(string userEmail, string userPassword, Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, userEmail, userPassword, userGroupKey);

        var createDataTypeModel = GenerateCreateDataTypeRequestModel();

        return await Client.PostAsync(Url, JsonContent.Create(createDataTypeModel));
    }

    private CreateDataTypeRequestModel GenerateCreateDataTypeRequestModel()
    {
        return new CreateDataTypeRequestModel
        {
            Id = Guid.NewGuid(),
            ParentId = null,
            Name = "TestName",
            PropertyEditorAlias = "Umbraco.Label",
            PropertyEditorUiAlias = "Umb.PropertyEditorUi.Label",
            Values = new List<DataTypePropertyPresentationModel>
            {
                new DataTypePropertyPresentationModel
                {
                    Alias = "ValueAlias",
                    Value = "TestValue"
                }
            }
        };
    }

}
