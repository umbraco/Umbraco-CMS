using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

/// <summary>
/// Deleting a locked data type is denied. The id doesn't exist, so reaching the action would return 404 instead.
/// </summary>
public class DeletingADataTypeIsForbiddenTests : EnvironmentPermissionsApiTest<DeleteDataTypeController>
{
    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }
        = x => x.Delete(CancellationToken.None, Guid.Empty);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDataTypesConfigurator>();

    [Test]
    public async Task Delete_Returns_Forbidden()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Locks data types, and nothing else.
    /// </summary>
    private sealed class LockDataTypesConfigurator : IEnvironmentPermissionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(IEnvironmentPermissionsBuilder builder)
            => builder.Deny(Constants.Permissions.Entities.DataType.Create, Constants.Permissions.Entities.DataType.Update, Constants.Permissions.Entities.DataType.Delete);
    }
}
