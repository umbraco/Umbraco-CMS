using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Server;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// A restricted entity type is reported operation by operation rather than as locked or not, so a partial lock
/// reaches the backoffice as one. This is what the backoffice disables its buttons from.
/// </summary>
public class ReportedOperationsFollowWhatWasBlockedTests : SchemaLockdownApiTest<SchemaLockdownServerController>
{
    protected override Expression<Func<SchemaLockdownServerController, object>> MethodSelector { get; set; }
        = x => x.SchemaLockdown(CancellationToken.None);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypeCreateAndUpdateConfigurator>();

    [Test]
    public async Task Only_The_Blocked_Operations_Are_Reported_As_Denied()
    {
        HttpResponseMessage response = await Client.GetAsync(Url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        ServerSchemaLockdownResponseModel? model =
            await response.Content.ReadFromJsonAsync<ServerSchemaLockdownResponseModel>();

        ServerSchemaLockdownEntityTypeResponseModel? dataType = model?.RestrictedEntityTypes
            .FirstOrDefault(x => x.EntityType == Constants.UdiEntityType.DataType);

        Assert.That(dataType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dataType!.Create, Is.False);
            Assert.That(dataType.Update, Is.False);
            Assert.That(dataType.Delete, Is.True);
        });
    }

    /// <summary>
    /// Denies creating and updating data types, leaving deletion alone.
    /// </summary>
    private sealed class LockDataTypeCreateAndUpdateConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Block(Constants.UdiEntityType.DataType, SchemaOperation.Create, SchemaOperation.Update);
    }
}
