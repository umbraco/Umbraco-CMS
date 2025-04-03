using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Install;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Install;

public class SetupInstallControllerTests : ManagementApiUserGroupTestBase<SetupInstallController>
{
    protected override Expression<Func<SetupInstallController, object>> MethodSelector =>
        x => x.Setup(CancellationToken.None, null);

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
        InstallRequestModel installRequestModel = new()
        {
            User = new UserInstallRequestModel
            {
                Name = "Tester", Email = "Test@emails.test", Password = "123457890"
            },
            Database = new DatabaseInstallRequestModel { Id = Guid.NewGuid(), ProviderName = "TestProviderName" },
            TelemetryLevel = TelemetryLevel.Basic
        };
        return await Client.PostAsync(Url, JsonContent.Create(installRequestModel));
    }
}
