using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary.Tree;

public class ChildrenDictionaryTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDictionaryTreeController>
{
    protected override Expression<Func<ChildrenDictionaryTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, Guid.NewGuid(), 0, 100);


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
        ExpectedStatusCode = HttpStatusCode.OK
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
