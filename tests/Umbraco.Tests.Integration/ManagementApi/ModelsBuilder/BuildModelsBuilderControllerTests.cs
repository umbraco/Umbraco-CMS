using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.ModelsBuilder;

public class BuildModelsBuilderControllerTests : ManagementApiUserGroupTestBase<BuildModelsBuilderController>
{
    protected override Expression<Func<BuildModelsBuilderController, object>> MethodSelector =>
        x => x.BuildModels(CancellationToken.None);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.PostAsync(Url, null);
}
