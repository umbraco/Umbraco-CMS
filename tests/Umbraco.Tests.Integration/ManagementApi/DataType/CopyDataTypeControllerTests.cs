using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class CopyDataTypeControllerTests : DataTypeTestBase<CopyDataTypeController>
{
    private Guid _dataTypeId;

    private Guid _targetId;

    protected override Expression<Func<CopyDataTypeController, object>> MethodSelector =>
        x => x.Copy(_dataTypeId, null);

    [SetUp]
    public void Setup()
    {
        _dataTypeId = CreateDataType();
        _targetId = CreateDataTypeFolder();
    }

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
        CopyDataTypeRequestModel copyDataTypeRequestModel =
            new() { TargetId = _targetId };

        return await Client.PostAsync(Url, JsonContent.Create(copyDataTypeRequestModel));
    }
}
