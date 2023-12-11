using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

public class ByKeyDataTypeFolderControllerTests : ManagementApiUserGroupTestBase<ByKeyDataTypeFolderController>
{
    protected override Expression<Func<ByKeyDataTypeFolderController, object>> MethodSelector =>
        x => x.ByKey(Guid.NewGuid());

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
}
