using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.HealthCheck;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.HealthCheck;

public class ExecuteActionHealthCheckControllerTests : ManagementApiUserGroupTestBase<ExecuteActionHealthCheckController>
{
    private Guid _dataIntegrityHealthCheckId;

    private HealthCheckCollection HealthChecks => GetRequiredService<HealthCheckCollection>();

    [SetUp]
    public async Task Setup()
    {
        var dataIntegrityCheck = HealthChecks.FirstOrDefault(x => x.Name.Contains("Data Integrity", StringComparison.OrdinalIgnoreCase));
        _dataIntegrityHealthCheckId = dataIntegrityCheck.Id;
    }

    protected override Expression<Func<ExecuteActionHealthCheckController, object>> MethodSelector =>
        x => x.ExecuteAction(CancellationToken.None, null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        HealthCheckActionRequestModel healthCheckActionRequest =
            new() { HealthCheck = new ReferenceByIdModel(_dataIntegrityHealthCheckId), ValueRequired = false, Alias = "fixContentPaths" };
        return await Client.PostAsync(Url, JsonContent.Create(healthCheckActionRequest));
    }
}
