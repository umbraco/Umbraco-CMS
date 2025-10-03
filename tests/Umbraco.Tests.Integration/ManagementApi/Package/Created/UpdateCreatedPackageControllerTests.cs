using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Package.Created;
using Umbraco.Cms.Api.Management.ViewModels.Package;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Package.Created;

public class UpdateCreatedPackageControllerTests : ManagementApiUserGroupTestBase<UpdateCreatedPackageController>
{
    protected override Expression<Func<UpdateCreatedPackageController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.NewGuid(), null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdatePackageRequestModel updatePackageRequestModel =
            new() { PackagePath = "PackagePath", Name = "TestPackageNameUpdated" };

        return await Client.PutAsync(Url, JsonContent.Create(updatePackageRequestModel));
    }
}
