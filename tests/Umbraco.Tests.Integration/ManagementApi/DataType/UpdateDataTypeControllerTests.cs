using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class UpdateDataTypeControllerTests : DataTypeTestBase<UpdateDataTypeController>
{
    private Guid _dataTypeId;

    protected override Expression<Func<UpdateDataTypeController, object>> MethodSelector =>
        x => x.Update(_dataTypeId, null);

    [SetUp]
    public void Setup()
    {
        _dataTypeId = CreateDataType();
    }

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateDataTypeRequestModel updateDataTypeRequestModel =
            new()
            {
                Name = "TestNameUpdated",
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

        return await Client.PutAsync(Url, JsonContent.Create(updateDataTypeRequestModel));
    }
}
