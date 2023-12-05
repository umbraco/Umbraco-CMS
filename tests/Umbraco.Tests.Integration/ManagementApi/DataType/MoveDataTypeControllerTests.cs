using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class MoveDataTypeControllerTests : DataTypeTestBase<MoveDataTypeController>
{
    private Guid _dataTypeId;

    private Guid _targetId;

    protected override Expression<Func<MoveDataTypeController, object>> MethodSelector =>
        x => x.Move(_dataTypeId, null);

    [SetUp]
    public void Setup()
    {
        _dataTypeId = CreateDataType();
        _targetId = CreateDataTypeFolder();
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
        MoveDataTypeRequestModel moveDataTypeRequestModel =
            new() { TargetId = _targetId };

        return await Client.PostAsync(Url, JsonContent.Create(moveDataTypeRequestModel));
    }
}
