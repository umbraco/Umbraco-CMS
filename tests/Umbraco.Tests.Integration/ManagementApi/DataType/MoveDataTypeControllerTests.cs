using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class MoveDataTypeControllerTests : ManagementApiUserGroupTestBase<MoveDataTypeController>
{
    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private Guid _dataTypeKey;
    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        var responseFolder = await DataTypeContainerService.CreateAsync(Guid.NewGuid(), "TestFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        _folderKey = responseFolder.Result.Key;

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.ListView)
                .Done()
            .Build();
        var responseDataType = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        _dataTypeKey = responseDataType.Result.Key;
    }

    protected override Expression<Func<MoveDataTypeController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _dataTypeKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveDataTypeRequestModel moveDataTypeRequestModel = new() { Target = new ReferenceByIdModel(_folderKey) };

        return await Client.PutAsync(Url, JsonContent.Create(moveDataTypeRequestModel));
    }
}
