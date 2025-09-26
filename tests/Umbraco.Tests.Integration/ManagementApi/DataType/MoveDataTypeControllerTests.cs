using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

[TestFixture]
public class MoveDataTypeControllerTests : ManagementApiUserGroupTestBase<MoveDataTypeController>
{
    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();
    private Guid _originalId;
    private Guid _targetId;

    [SetUp]
    public async Task Setup()
    {
        var response1 = await DataTypeContainerService.CreateAsync(Guid.NewGuid(), "OriginalFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        var response2 = await DataTypeContainerService.CreateAsync(Guid.NewGuid(), "TargetFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        _originalId = response1.Result.Key;
        _targetId = response2.Result.Key;
    }

    protected override Expression<Func<MoveDataTypeController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _originalId, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveDataTypeRequestModel moveDataTypeRequestModel = new() { Target = new ReferenceByIdModel(_targetId) };

        return await Client.PutAsync(Url, JsonContent.Create(moveDataTypeRequestModel));
    }
}
