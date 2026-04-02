using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Install;
using Umbraco.Cms.Api.Management.ViewModels.Installer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Install;

public class ValidateDatabaseInstallControllerTests : ManagementApiUserGroupTestBase<ValidateDatabaseInstallController>
{
    protected override Expression<Func<ValidateDatabaseInstallController, object>> MethodSelector =>
        x => x.ValidateDatabase(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.PreconditionRequired
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        DatabaseInstallRequestModel databaseInstallRequestModel = new()
        {
            Id = Guid.NewGuid(), ProviderName = "TestProviderName",
        };
        return await Client.PostAsync(Url, JsonContent.Create(databaseInstallRequestModel));
    }
}
