using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class CreateDataTypeControllerTests : ManagementApiUserGroupTestBase<CreateDataTypeController>
{
    protected override Expression<Func<CreateDataTypeController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        CreateDataTypeRequestModel createDataTypeRequestModel = new()
        {
            Id = Guid.NewGuid(),
            Parent = null,
            Name = "TestDataType",
            EditorAlias = "Umbraco.CheckBoxList",
            EditorUiAlias = "Umb.PropertyEditorUi.CheckBoxList",
        };

        return await Client.PostAsync(Url, JsonContent.Create(createDataTypeRequestModel));
    }
}
