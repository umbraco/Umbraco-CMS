using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class UpdateDataTypeControllerTests : ManagementApiUserGroupTestBase<UpdateDataTypeController>
{
    protected override Expression<Func<UpdateDataTypeController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.NewGuid(), null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        ExpectedStatusCode = HttpStatusCode.NotFound
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
                EditorAlias = "Umbraco.Label",
                EditorUiAlias = "Umb.PropertyEditorUi.Label",
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
