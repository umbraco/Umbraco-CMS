using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

public class ByKeyDataTypeFolderControllerTests : ManagementApiUserGroupTestBase<ByKeyDataTypeFolderController>
{
    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        var response = await DataTypeContainerService.CreateAsync(Guid.NewGuid(), "TestFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
        _folderKey = response.Result.Key;
    }

    protected override Expression<Func<ByKeyDataTypeFolderController, object>> MethodSelector =>
        x => x.ByKey(CancellationToken.None, _folderKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
}
