using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Server;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// The endpoint reports only the permission verbs that were denied, so a partial lock reaches the backoffice.
/// </summary>
public class ReportedPermissionsFollowWhatWasBlockedTests : SchemaLockdownApiTest<SchemaLockdownServerController>
{
    protected override Expression<Func<SchemaLockdownServerController, object>> MethodSelector { get; set; }
        = x => x.SchemaLockdown(CancellationToken.None);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypeCreateAndUpdateConfigurator>();

    [Test]
    public async Task Only_The_Denied_Permissions_Are_Reported()
    {
        HttpResponseMessage response = await Client.GetAsync(Url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        ServerSchemaLockdownResponseModel? model =
            await response.Content.ReadFromJsonAsync<ServerSchemaLockdownResponseModel>();

        Assert.That(model, Is.Not.Null);
        Assert.That(
            model!.DeniedPermissions,
            Is.EquivalentTo(new[] { Constants.Permissions.DataType.Create, Constants.Permissions.DataType.Update }));
    }

    /// <summary>
    /// Denies creating and updating data types, leaving deletion alone.
    /// </summary>
    private sealed class LockDataTypeCreateAndUpdateConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Deny(Constants.Permissions.DataType.Create, Constants.Permissions.DataType.Update);
    }
}
